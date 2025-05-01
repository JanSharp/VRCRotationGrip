using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Components;
using UnityEditor;
using UdonSharpEditor;
using System.Linq;

namespace JanSharp
{
    [InitializeOnLoad]
    public static class RotationGripOnBuild
    {
        static RotationGripOnBuild() => JanSharp.OnBuildUtil.RegisterType<RotationGrip>(OnBuild);

        private static bool OnBuild(RotationGrip rotationGrip)
        {
            float initialDistance = rotationGrip.toRotate.InverseTransformDirection(rotationGrip.transform.position - rotationGrip.toRotate.position).magnitude;

            SerializedObject rotationGripProxy = new SerializedObject(rotationGrip);
            rotationGripProxy.FindProperty(nameof(RotationGrip.pickup)).objectReferenceValue = rotationGrip.GetComponent<VRC_Pickup>();
            rotationGripProxy.FindProperty(nameof(RotationGrip.initialLocalRotation)).quaternionValue = rotationGrip.toRotate.localRotation;
            rotationGripProxy.FindProperty(nameof(RotationGrip.maximumRotationDeviation)).floatValue = Mathf.Abs(rotationGrip.maximumRotationDeviation);
            rotationGripProxy.FindProperty(nameof(RotationGrip.initialDistance)).floatValue = initialDistance;
            rotationGripProxy.ApplyModifiedProperties();

            SerializedObject transformProxy = new SerializedObject(rotationGrip.transform);
            transformProxy.FindProperty("m_LocalPosition").vector3Value
                = EditorUtil.WorldToLocalPosition(rotationGrip.transform, rotationGrip.GetSnappedPosition(initialDistance));
            transformProxy.FindProperty("m_LocalRotation").quaternionValue
                = EditorUtil.WorldToLocalRotation(rotationGrip.transform, rotationGrip.GetSnappedRotation());
            transformProxy.ApplyModifiedProperties();

            return true;
        }
    }

    [CanEditMultipleObjects]
    [CustomEditor(typeof(RotationGrip))]
    public class RotationGripEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(targets))
                return;
            EditorGUILayout.Space();
            base.OnInspectorGUI(); // draws public/serializable fields
            EditorGUILayout.Space();

            if (GUILayout.Button(new GUIContent("Snap in Line", "Snap to the back of the Transform 'To Rotate'. "
                + "This script relies on this pickup object being perfectly in line with the Transform it is rotating, "
                + "so this button allows you to snap it in place before entering play mode.")))
            {
                foreach (var rotationGrip in targets.Cast<RotationGrip>())
                {
                    float distance = rotationGrip.toRotate.TransformDirection(
                        rotationGrip.transform.position - rotationGrip.toRotate.position
                    ).magnitude;
                    SerializedObject transformProxy = new SerializedObject(rotationGrip.transform);
                    transformProxy.FindProperty("m_LocalPosition").vector3Value
                        = EditorUtil.WorldToLocalPosition(rotationGrip.transform, rotationGrip.GetSnappedPosition(distance));
                    transformProxy.FindProperty("m_LocalRotation").quaternionValue
                        = EditorUtil.WorldToLocalRotation(rotationGrip.transform, rotationGrip.GetSnappedRotation());
                    transformProxy.ApplyModifiedProperties();
                }
            }

            EditorUtil.ConditionalButton(new GUIContent("Configure VRC Pickup", "Configures the attached VRC Pickup "
                + "and Rigidbody components. Sets: useGravity = false; isKinematic = true; ExactGrip = null; orientation = Grip;\n"
                + "Setting orientation to grip and ExactGrip to null makes the pickup stay where it is while being picked up => "
                + "it doesn't move to the hand."),
                targets.Cast<RotationGrip>()
                    .Select(rotationGrip => (
                        rotationGrip,
                        pickup: rotationGrip.GetComponent<VRCPickup>(),
                        rigidbody: rotationGrip.GetComponent<Rigidbody>()
                    ))
                    .Where(data => {
                        return data.pickup.ExactGrip != null
                            || data.pickup.orientation != VRC_Pickup.PickupOrientation.Grip
                            || data.rigidbody.useGravity
                            || !data.rigidbody.isKinematic;
                    }),
                allData => {
                    SerializedObject rigidbodiesProxy = new SerializedObject(allData.Select(d => d.rigidbody).ToArray());
                    rigidbodiesProxy.FindProperty("m_UseGravity").boolValue = false;
                    rigidbodiesProxy.FindProperty("m_IsKinematic").boolValue = true;
                    rigidbodiesProxy.ApplyModifiedProperties();

                    SerializedObject pickupsProxy = new SerializedObject(allData.Select(d => d.pickup).ToArray());
                    pickupsProxy.FindProperty("ExactGrip").objectReferenceValue = null;
                    pickupsProxy.FindProperty("orientation").intValue = (int)VRC_Pickup.PickupOrientation.Grip;
                    pickupsProxy.ApplyModifiedProperties();
                }
            );
        }
    }
}
