﻿using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Components;

// TODO: interpolate to hand in VR
// TODO: option to always interpolate, regardless of VR or not
// TODO: improve interpolation by taking the previous interpolation time into consideration
// TODO: fix toRotate local rotation being all kinds of funky

namespace JanSharp
{
    [RequireComponent(typeof(VRCPickup))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class RotationGrip : UdonSharpBehaviour
    {
        public Transform toRotate;
        [Tooltip("Maximum amount of degrees the object to rotate is allowed to deviate from the original local rotation. 180 and above means unlimited, 0 or below means not at all.")]
        public float maximumRotationDeviation = 180f;
        [Tooltip("When true the object To Rotate will only rotate around its up axis (the green arrow).")]
        public bool rotateAroundSingleAxis;

        [HideInInspector] public UpdateManager updateManager;
        [HideInInspector] public Transform dummyTransform;
        [HideInInspector] public VRC_Pickup pickup;
        [HideInInspector] public Quaternion initialLocalRotation;
        [HideInInspector] public float initialDistance;
        private float nextSyncTime;
        private const float SyncInterval = 0.2f;
        private const float LerpDuration = SyncInterval + 0.05f;
        private bool isRegistered;
        private bool isReceiving;
        private bool isLerping;
        private float lerpStartTime;
        private float currentLerpDuration;
        private Quaternion lerpStartRotation;
        private Quaternion prevRotation;
        private VRCPlayerApi holdingPlayer;
        private VRCPlayerApi HoldingPlayer
        {
            get => holdingPlayer;
            set
            {
                holdingPlayer = value;
                holdingPlayerIsInVR = value.IsUserInVR();
            }
        }
        private bool holdingPlayerIsInVR;

        /// <summary>
        /// <para>bit 0: is held</para>
        /// <para>bit 1: 0 means left hand, 1 means right hand (only used when the holding player is in VR)</para>
        /// </summary>
        [UdonSynced] private byte syncedData;
        private const byte IsHeldFlag = 1 << 0;
        private const byte HeldHandFlag = 1 << 1;
        private bool currentlyHeld; // synced through syncedData
        private HumanBodyBones currentHandBone; // synced through syncedData
        /// <summary>
        /// <para>Used as the target global rotation for interpolation.
        /// Global is ultimately better because it allows for RotationGrips to be used where the
        /// parent transform rotation can move but isn't synced. And it is ultimately only a little
        /// bit more complex.</para>
        /// <para>If the holding user is in VR and is currently holding the grip then this is
        /// used as the rotation offset between the held hand rotation and the pickup rotation.</para>
        /// </summary>
        [UdonSynced] private Quaternion syncedRotation;
        /// <summary>
        /// The offset from the held hand position and the pickup position;
        /// Only used when the holding user is in VR.
        /// </summary>
        [UdonSynced] private Vector3 syncedPosition;

        private byte ToHeldHandFlag(HumanBodyBones bone)
        {
            if (bone == HumanBodyBones.RightHand)
                return HeldHandFlag;
            return 0;
        }

        private HumanBodyBones ToHeldHandBone(byte flags)
        {
            if ((flags & HeldHandFlag) != 0)
                return HumanBodyBones.RightHand;
            return HumanBodyBones.LeftHand;
        }

        // for UpdateManager
        private int customUpdateInternalIndex;

        public Vector3 GetSnappedPosition(float distance)
        {
            return toRotate.position + toRotate.TransformDirection(Vector3.back * distance);
        }

        public Quaternion GetSnappedRotation() => toRotate.rotation;

        private void Snap(float distance)
        {
            this.transform.position = GetSnappedPosition(distance);
            this.transform.rotation = GetSnappedRotation();
        }

        private void SnapBack() => Snap(initialDistance);

        public override void OnPickup()
        {
            HoldingPlayer = Networking.LocalPlayer;
            Networking.SetOwner(HoldingPlayer, this.gameObject);
            isReceiving = false;
            currentlyHeld = true;
            currentHandBone = pickup.currentHand == VRC_Pickup.PickupHand.Right ? HumanBodyBones.RightHand : HumanBodyBones.LeftHand;
            RequestSerialization();
            Register();
            if (holdingPlayerIsInVR)
                nextSyncTime = Time.time + SyncInterval;
        }

        public override void OnDrop()
        {
            currentlyHeld = false;
            RequestSerialization();
            SnapBack();
            Deregister();
        }

        public void CustomUpdate()
        {
            if (isReceiving)
            {
                if (holdingPlayerIsInVR && currentlyHeld)
                {
                    // figure out the position of the pickup based on the current bone position
                    this.transform.SetPositionAndRotation(
                        holdingPlayer.GetBonePosition(currentHandBone),
                        holdingPlayer.GetBoneRotation(currentHandBone)
                    );
                    this.transform.position += this.transform.TransformDirection(syncedPosition);
                    this.transform.rotation *= syncedRotation;
                    LookAtThisTransform();
                }
                else // when not currentlyHeld interpolate instead
                {
                    var percent = (Time.time - lerpStartTime) / currentLerpDuration;
                    Quaternion extraRotationSinceLastFrame = Quaternion.Inverse(prevRotation) * toRotate.rotation;
                    toRotate.rotation = Quaternion.Lerp(lerpStartRotation, syncedRotation, percent) * extraRotationSinceLastFrame;
                    if (percent >= 1f)
                    {
                        SnapBack();
                        pickup.pickupable = true;
                        isLerping = false;
                        Deregister();
                    }
                    prevRotation = toRotate.rotation;
                }
            }
            else
            {
                LookAtThisTransform();
                if (!holdingPlayerIsInVR && Time.time >= nextSyncTime)
                {
                    RequestSerialization();
                    nextSyncTime = Time.time + SyncInterval;
                }
            }
        }

        private void LookAtThisTransform()
        {
            var parent = this.transform.parent;
            Vector3 worldUp = parent != null ? parent.up : Vector3.up;
            toRotate.LookAt(toRotate.position * 2 - this.transform.position, worldUp);

            Quaternion deviation;
            if (rotateAroundSingleAxis)
            {
                Vector3 initialLocalDir = initialLocalRotation * Vector3.forward;
                Vector3 initialLocalUp = initialLocalRotation * Vector3.up;
                Vector3 currentLocalDir = toRotate.localRotation * Vector3.forward;

                Vector3 parallelDir = currentLocalDir - (initialLocalUp * Vector3.Dot(currentLocalDir, initialLocalUp));
                deviation = Quaternion.FromToRotation(initialLocalDir, parallelDir);
            }
            else
            {
                deviation = Quaternion.Inverse(initialLocalRotation) * toRotate.localRotation;
            }

            float angle;
            Vector3 axis;
            deviation.ToAngleAxis(out angle, out axis);
            float sign = 1f;
            if (angle >= 180f)
            {
                angle = 360f - angle;
                sign = -1f;
            }
            if (angle > maximumRotationDeviation)
            {
                deviation = Quaternion.AngleAxis(maximumRotationDeviation * sign, axis);
                toRotate.localRotation = initialLocalRotation * deviation;
            }
            else if (rotateAroundSingleAxis)
            {
                toRotate.localRotation = initialLocalRotation * deviation;
            }
        }

        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            // just for safety because I do not trust VRC
            // would be hilarious if this ended up running after OnDeserialization
            // causing the entire logic to be pointless/broken. Funny indeed
            pickup.pickupable = true;
            SnapBack();
        }

        public override void OnPreSerialization()
        {
            syncedData = (byte)((currentlyHeld ? IsHeldFlag : 0) + ToHeldHandFlag(currentHandBone));
            if (holdingPlayerIsInVR && currentlyHeld)
            {
                Vector3 bonePosition = holdingPlayer.GetBonePosition(currentHandBone);
                Quaternion boneRotation = holdingPlayer.GetBoneRotation(currentHandBone);
                syncedRotation = Quaternion.Inverse(boneRotation) * this.transform.rotation;
                dummyTransform.SetPositionAndRotation(bonePosition, boneRotation);
                syncedPosition = dummyTransform.InverseTransformDirection(this.transform.position - bonePosition);
            }
            else
                syncedRotation = toRotate.rotation;
        }

        public override void OnDeserialization()
        {
            currentlyHeld = (syncedData & IsHeldFlag) != 0;
            currentHandBone = ToHeldHandBone(syncedData);
            isReceiving = true;
            Register();
            if (currentlyHeld)
            {
                isLerping = false;
                pickup.pickupable = false;
                HoldingPlayer = Networking.GetOwner(this.gameObject);
            }
            if (!holdingPlayerIsInVR || !currentlyHeld) // always interpolate for desktop, also interpolate for VR when not held
            {
                lerpStartRotation = toRotate.rotation;
                prevRotation = toRotate.rotation;
                if (isLerping)
                {
                    float time = Time.time;
                    float percent = (Time.time - lerpStartTime) / currentLerpDuration;
                    float remainingPercentage = 1 - percent;
                    if (remainingPercentage == 1)
                    {
                        currentLerpDuration = LerpDuration;
                        lerpStartTime = time;
                    }
                    else
                    {
                        currentLerpDuration = -(LerpDuration / (1 - remainingPercentage));
                        lerpStartTime = time + LerpDuration - currentLerpDuration;
                    }
                    #region math
                    // ok, so, math...
                    // we need to know the remaining percentage of the duration of the current on going interpolation
                    // then we need to figure out the lerp start time and the current duration such that the current
                    // `Time.time` is at the same percentage as before, except flipped, and that the ultimate stop time is
                    // exactly `LerpDuration` away from the current `Time.time`
                    //
                    // alright, so it's a bit of formula shuffling
                    // lerpStartTime + currentLerpDuration = time + LerpDuration;
                    // &
                    // lerpStartTime + (remainingPercentage * currentLerpDuration) = time;
                    //
                    // The unknown variables are `lerpStartTime` and `currentLerpDuration`.
                    //
                    // The way to solve this kind of equation is to reformulate one of them to say
                    // either `lerpStartTime = ...` or `currentLerpDuration = ...`
                    // and then insert the right hand side of that formula into the other formula
                    //
                    // then you can reformulate that one such that the only
                    // unknown variable in it is alone on the left hand side
                    //
                    // we now know one of the 2 unknowns, now we can use the very first reformulated equation
                    // to figure out the second one
                    //
                    // and done
                    //
                    // the reason why I remember this is because I loved these kinds of problems at school
                    //
                    //
                    // ok, first step:
                    // lerpStartTime + currentLerpDuration = time + LerpDuration;
                    // lerpStartTime = time + LerpDuration - currentLerpDuration;
                    //
                    // second step:
                    // lerpStartTime + (remainingPercentage * currentLerpDuration) = time;
                    // time + LerpDuration - currentLerpDuration + (remainingPercentage * currentLerpDuration) = time;
                    // now extract currentLerpDuration somehow
                    // LerpDuration - currentLerpDuration + (remainingPercentage * currentLerpDuration) = 0;
                    // currentLerpDuration + (remainingPercentage * currentLerpDuration) = -LerpDuration;
                    // (currentLerpDuration + (remainingPercentage * currentLerpDuration)) / remainingPercentage = (-LerpDuration) / remainingPercentage;
                    // currentLerpDuration / remainingPercentage + (remainingPercentage * currentLerpDuration) / remainingPercentage = (-LerpDuration) / remainingPercentage;
                    // currentLerpDuration / remainingPercentage + currentLerpDuration = (-LerpDuration) / remainingPercentage;

                    // AAA I can't remember! I can't remember how to math
                    // I could try to just logic it out, without math, but I'm too invested now
                    // plus I already know that logic wouldn't me much simpler

                    // ok, so, we have this:
                    // time + LerpDuration - currentLerpDuration + (remainingPercentage * currentLerpDuration) = time;
                    // and we want to know currentLerpDuration
                    // which means we need to somehow get `currentLerpDuration = ...` out of it
                    // but how... ok, well, `time` is easy to get rid of
                    // LerpDuration - currentLerpDuration + (remainingPercentage * currentLerpDuration) = 0;
                    // LerpDuration / remainingPercentage - currentLerpDuration / remainingPercentage + currentLerpDuration = 0;
                    // ok, I give up for now
                    // I still do want to figure it out myself, but right now I want to move on
                    // y - x + (z * x) = 0
                    // x + (z * x) = -y       | -y
                    // (1 * x) + (z * x) = -y
                    // yea idk, let's just accept the solution
                    // x = (-y) / (-1 + z)
                    // no I will not just accept it
                    // (1 * x) / (-1 + z) + (z * x) / (-1 + z) = -y / (-1 + z)
                    // so basically what you're telling me is that
                    // (1 * x) / (-1 + z) + (z * x) / (-1 + z)
                    // is ultimately the same as just `x`
                    // uh huh
                    // I don't get it
                    //
                    // (a + b) * (a - b)
                    // a * (a - b) + b * (a - b)
                    // a * a - a * b + b * a - b * b
                    // a * a - b * b
                    // nope that's not what I'm looking for
                    //
                    // alright, moving on
                    // currentLerpDuration = -(LerpDuration / (1 - remainingPercentage));
                    // lerpStartTime = time + LerpDuration - currentLerpDuration;
                    #endregion
                }
                else
                {
                    isLerping = true;
                    currentLerpDuration = LerpDuration;
                    lerpStartTime = Time.time;
                }
            }
        }

        public void Register()
        {
            if (isRegistered)
                return;
            isRegistered = true;
            updateManager.Register(this);
        }

        public void Deregister()
        {
            if (!isRegistered)
                return;
            isRegistered = false;
            updateManager.Deregister(this);
        }
    }
}
