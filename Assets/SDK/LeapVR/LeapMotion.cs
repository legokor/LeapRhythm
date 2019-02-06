using Leap;
using System.Collections.Generic;
using UnityEngine;

namespace LeapVR {
    /// <summary>
    /// Leap Motion handling simplified.
    /// </summary>
    [DefaultExecutionOrder(int.MinValue + 1000)]
    [AddComponentMenu("Leap VR/Leap Motion")]
    public class LeapMotion : Singleton<LeapMotion> {
        public bool HeadMounted = true;
        [Tooltip("Lower values of hand detection bounds.")]
        public Vector3 LeapLowerBounds = new Vector3(-200, 100, -112.5f);
        [Tooltip("Upper values of hand detection bounds.")]
        public Vector3 LeapUpperBounds = new Vector3(200, 300, 112.5f);

        /// <summary>
        /// Connected Leap Motion device.
        /// </summary>
        Controller Device;

        /// <summary>
        /// The Leap Motion frame just before the game's frame update.
        /// </summary>
        Frame LastFrame = new Frame();

        /// <summary>
        /// The last frame in the Leap Motion SDK's format.
        /// </summary>
        public Frame RawFrame { get { return LastFrame; } }

        /// <summary>
        /// A position indicating unavailable Leap Motion data (e.g. no hands are detected).
        /// </summary>
        public static readonly Vector2 NotAvailable = new Vector2(-1, -1);

        /// <summary>
        /// Connect to the device automatically on creation.
        /// </summary>
        void Awake() {
            Device = new Controller();
        }

        /// <summary>
        /// Safely disconnect the device after use.
        /// </summary>
        void OnDestroy() {
            if (Device.IsConnected)
                Device.ClearPolicy(Controller.PolicyFlag.POLICY_OPTIMIZE_HMD);
            Device.StopConnection();
        }

        /// <summary>
        /// Returns if a connected Leap Motion has been found.
        /// </summary>
        public bool Connected {
            get {
                return Device.IsConnected;
            }
        }

        /// <summary>
        /// Check if the user is using the controller.
        /// </summary>
        /// <returns>True if there are any hands detected</returns>
        public bool IsUsed() {
            return LastFrame.Hands.Count != 0;
        }

        /// <summary>
        /// Get count of hands.
        /// </summary>
        /// <returns>The number of hands the device detects</returns>
        public int GetHandCount() {
            return LastFrame.Hands.Count;
        }

        /// <summary>
        /// Raw palm position data.
        /// </summary>
        /// <param name="HandID">Hand ID</param>
        /// <returns>Palm position, or (-1, -1) if there's no hand</returns>
        public Vector3 PalmPosition(int HandID = 0) {
            if (LastFrame.Hands.Count > HandID) {
                Vector Pos = LastFrame.Hands[HandID].PalmPosition;
                return new Vector3(Pos.x, Pos.y, Pos.z);
            } else
                return NotAvailable;
        }

        /// <summary>
        /// Palm position on screen, on a vertical plane.
        /// </summary>
        /// <param name="HandID">Hand ID</param>
        /// <returns>Palm position on screen, or (-1, -1) if there's no hand</returns>
        public Vector2 PalmOnScreenXY(int HandID = 0) {
            List<Hand> Hands = LastFrame.Hands;
            if (Hands.Count > HandID) {
                Hand CheckedHand = Hands[HandID];
                Vector2 FromLeap = new Vector2(CheckedHand.PalmPosition.x, -CheckedHand.PalmPosition.y + LeapLowerBounds.y + LeapUpperBounds.y);
                return new Vector2(
                    (Mathf.Clamp(FromLeap.x, LeapLowerBounds.x, LeapUpperBounds.x) - LeapLowerBounds.x) / (LeapUpperBounds.x - LeapLowerBounds.x) * Screen.width,
                    (Mathf.Clamp(FromLeap.y, LeapLowerBounds.y, LeapUpperBounds.y) - LeapLowerBounds.y) / (LeapUpperBounds.y - LeapLowerBounds.y) * Screen.height);
            } else {
                return NotAvailable;
            }
        }

        /// <summary>
        /// Palm position on or off screen, on a vertical plane.
        /// </summary>
        /// <param name="HandID">Hand ID</param>
        /// <returns>Palm position on screen, or (-1, -1) if there's no hand</returns>
        public Vector2 PalmOnScreenXYUnclamped(int HandID = 0) {
            List<Hand> Hands = LastFrame.Hands;
            if (Hands.Count > HandID) {
                Hand CheckedHand = Hands[HandID];
                Vector2 FromLeap = new Vector2(CheckedHand.PalmPosition.x, -CheckedHand.PalmPosition.y + LeapLowerBounds.y + LeapUpperBounds.y);
                return new Vector2((FromLeap.x - LeapLowerBounds.x) / (LeapUpperBounds.x - LeapLowerBounds.x) * Screen.width,
                    (FromLeap.y - LeapLowerBounds.y) / (LeapUpperBounds.y - LeapLowerBounds.y) * Screen.height);
            } else {
                return NotAvailable;
            }
        }

        /// <summary>
        /// Palm position on viewport, on a vertical plane.
        /// </summary>
        /// <param name="HandID">Hand ID</param>
        /// <returns>Palm position on viewport, or (-1, -1) if there's no hand</returns>
        public Vector2 PalmOnViewportXY(int HandID = 0) {
            List<Hand> Hands = LastFrame.Hands;
            if (Hands.Count > HandID) {
                Hand CheckedHand = Hands[HandID];
                Vector2 FromLeap = new Vector2(CheckedHand.PalmPosition.x, -CheckedHand.PalmPosition.y + LeapLowerBounds.y + LeapUpperBounds.y);
                return new Vector2(
                    (Mathf.Clamp(FromLeap.x, LeapLowerBounds.x, LeapUpperBounds.x) - LeapLowerBounds.x) / (LeapUpperBounds.x - LeapLowerBounds.x),
                    (Mathf.Clamp(FromLeap.y, LeapLowerBounds.y, LeapUpperBounds.y) - LeapLowerBounds.y) / (LeapUpperBounds.y - LeapLowerBounds.y));
            } else {
                return NotAvailable;
            }
        }

        /// <summary>
        /// Palm position on screen, on a horizontal plane.
        /// </summary>
        /// <param name="HandID">Hand ID</param>
        /// <returns>Palm position on screen, or (-1, -1) if there's no hand</returns>
        public Vector2 PalmOnScreenXZ(int HandID = 0) {
            List<Hand> Hands = LastFrame.Hands;
            if (Hands.Count > HandID) {
                Hand CheckedHand = Hands[HandID];
                return new Vector2(
                    (Mathf.Clamp(CheckedHand.PalmPosition.x, LeapLowerBounds.x, LeapUpperBounds.x) - LeapLowerBounds.x) /
                    (LeapUpperBounds.x - LeapLowerBounds.x) * Screen.width,
                    (Mathf.Clamp(CheckedHand.PalmPosition.z, LeapLowerBounds.z, LeapUpperBounds.z) - LeapLowerBounds.z) /
                    (LeapUpperBounds.z - LeapLowerBounds.z) * Screen.height);
            } else {
                return NotAvailable;
            }
        }

        /// <summary>
        /// Palm position on viewport, on a horizontal plane.
        /// </summary>
        /// <param name="HandID">Hand ID</param>
        /// <returns>Palm position on viewport, or (-1, -1) if there's no hand</returns>
        public Vector2 PalmOnViewportXZ(int HandID = 0) {
            List<Hand> Hands = LastFrame.Hands;
            if (Hands.Count > HandID) {
                Hand CheckedHand = Hands[HandID];
                return new Vector2(
                    (Mathf.Clamp(CheckedHand.PalmPosition.x, LeapLowerBounds.x, LeapUpperBounds.x) - LeapLowerBounds.x) / (LeapUpperBounds.x - LeapLowerBounds.x),
                    (Mathf.Clamp(CheckedHand.PalmPosition.z, LeapLowerBounds.z, LeapUpperBounds.z) - LeapLowerBounds.z) / (LeapUpperBounds.z - LeapLowerBounds.z));
            } else {
                return NotAvailable;
            }
        }

        /// <summary>
        /// Furthest tip on screen on a vertical plane.
        /// </summary>
        /// <param name="HandID">Hand ID</param>
        /// <returns>Furthest tip position on screen, or (-1, -1) if there's no hand</returns>
        public Vector2 SinglePointOnScreenXY(int HandID = 0) {
            List<Hand> Hands = LastFrame.Hands;
            if (Hands.Count > HandID) {
                Hand CurrentHand = Hands[HandID];
                Finger Furthest = CurrentHand.Fingers[0];
                foreach (Finger CheckedFinger in CurrentHand.Fingers)
                    if (Furthest.StabilizedTipPosition.z > CheckedFinger.StabilizedTipPosition.z)
                        Furthest = CheckedFinger;
                return new Vector2(
                    (Mathf.Clamp(Furthest.StabilizedTipPosition.x, LeapLowerBounds.x, LeapUpperBounds.x) - LeapLowerBounds.x) /
                    (LeapUpperBounds.x - LeapLowerBounds.x) * Screen.width,
                    (Mathf.Clamp(Furthest.StabilizedTipPosition.y, LeapLowerBounds.y, LeapUpperBounds.y) - LeapLowerBounds.y) /
                    (LeapUpperBounds.y - LeapLowerBounds.y) * Screen.height);
            } else {
                return NotAvailable;
            }
        }

        /// <summary>
        /// Planar hand movement delta calculation.
        /// </summary>
        /// <param name="CurrentPosition">Current screen position</param>
        /// <param name="LastPositionHolder">Holder for the last position</param>
        /// <returns>Hand movement on the plane in pixels or viewport scale</returns>
        public Vector2 ScreenDelta(Vector2 CurrentPosition, ref Vector2 LastPositionHolder) {
            if (CurrentPosition == NotAvailable || LastPositionHolder == NotAvailable) {
                LastPositionHolder = CurrentPosition;
                return NotAvailable;
            }
            Vector2 Delta = CurrentPosition - LastPositionHolder;
            LastPositionHolder = CurrentPosition;
            return Delta;
        }

        /// <summary>
        /// Extended fingers for a given hand.
        /// </summary>
        /// <param name="HandID">Hand ID</param>
        /// <returns>Extended finger count</returns>
        public int ExtendedFingers(int HandID = 0) {
            int Counter = 0;
            List<Hand> Hands = LastFrame.Hands;
            if (Hands.Count > HandID) {
                foreach (Finger CheckedFinger in Hands[HandID].Fingers)
                    if (CheckedFinger.IsExtended)
                        ++Counter;
            }
            return Counter;
        }

        /// <summary>
        /// Get the first detected left hand's ID or -1 if it's not found.
        /// </summary>
        /// <returns></returns>
        public int FirstLeftHand() {
            List<Hand> Hands = LastFrame.Hands;
            int HandCount = Hands.Count;
            for (int i = 0; i < HandCount; ++i)
                if (Hands[i].IsLeft)
                    return i;
            return -1;
        }

        /// <summary>
        /// Get the first detected right hand's ID or -1 if it's not found.
        /// </summary>
        /// <returns></returns>
        public int FirstRightHand() {
            List<Hand> Hands = LastFrame.Hands;
            int HandCount = Hands.Count;
            for (int i = 0; i < HandCount; ++i)
                if (!Hands[i].IsLeft)
                    return i;
            return -1;
        }

        /// <summary>
        /// Get pointing direction if the hand is pointing a direction, relative to the controller.
        /// </summary>
        /// <param name="HandID">Hand ID</param>
        /// <returns>Pointing direction vector for the given hand or null if it does not exist or point in a direction</returns>
        public Vector3? PointingDirection(int HandID = 0) {
            int Fingers = ExtendedFingers(HandID);
            if (((Fingers == 1 || Fingers == 2)) && LastFrame.Hands.Count > HandID) {
                foreach (Finger CheckedFinger in LastFrame.Hands[HandID].Fingers) {
                    if (CheckedFinger.IsExtended) {
                        Vector TipPos = CheckedFinger.Bone(Bone.BoneType.TYPE_DISTAL).Basis.translation,
                            PreTip = CheckedFinger.Bone(Bone.BoneType.TYPE_INTERMEDIATE).Basis.translation;
                        return new Vector3(TipPos.x - PreTip.x, TipPos.y - PreTip.y, TipPos.z - PreTip.z).normalized;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Get a hand's rotation by averaging where each finger's base points.
        /// </summary>
        /// <param name="HandID">Hand ID</param>
        /// <returns>Hand rotation</returns>
        public Quaternion? HandRotation(int HandID = 0) {
            if (LastFrame.Hands.Count > HandID) {
                Vector3 Pointing = Vector3.zero;
                foreach (Finger CheckedFinger in LastFrame.Hands[HandID].Fingers) {
                    if (CheckedFinger.Type != Finger.FingerType.TYPE_THUMB) {
                        Vector Base = CheckedFinger.Bone(CheckedFinger.IsExtended ? Bone.BoneType.TYPE_PROXIMAL : Bone.BoneType.TYPE_METACARPAL).Basis.translation,
                            End = CheckedFinger.Bone(CheckedFinger.IsExtended ? Bone.BoneType.TYPE_INTERMEDIATE : Bone.BoneType.TYPE_PROXIMAL).Basis.translation;
                        Pointing += new Vector3(End.x - Base.x, End.y - Base.y, End.z - Base.z);
                    }
                }
                if (Pointing != Vector3.zero) {
                    Vector PalmNormal = LastFrame.Hands[HandID].PalmNormal;
                    Vector3 Forward = Quaternion.LookRotation(Pointing, Vector3.up).eulerAngles;
                    Vector3 Normal = Quaternion.FromToRotation(Vector3.up, new Vector3(PalmNormal.x, PalmNormal.y, PalmNormal.z)).eulerAngles;
                    return Quaternion.Euler(-Forward.x, -Forward.y, -Normal.z);
                }
            }
            return null;
        }

        /// <summary>
        /// Poll the head mounted flag and get last Leap frame in each application frame.
        /// </summary>
        void Update() {
            if (HeadMounted && !Device.IsPolicySet(Controller.PolicyFlag.POLICY_OPTIMIZE_HMD))
                Device.SetPolicy(Controller.PolicyFlag.POLICY_OPTIMIZE_HMD);
            LastFrame = Device.Frame();
        }
    }
}