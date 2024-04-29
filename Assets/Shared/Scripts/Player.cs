using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Game.PhysicsEngine;
using Game.Logic.Characters;
using Game.Logic.Entities;

namespace Game.Logic
{


    /// <summary>
    /// The controller of the player's avatar.
    /// </summary>
    public class Player : MonoBehaviour
    {
        /// <summary>
        /// Quick class field to access the current controller from any other script.
        /// </summary>
        public static Player main;

        /// <summary>
        /// The current position of the player, in world space.
        /// </summary>
        public Vector3 position
        {
            get
            {
                return transform.position;
            } set
            {
                transform.position = value;
            }
        }

        /// <summary>
        /// The current forward vector of the player, in world space.
        /// </summary>
        public Vector3 direction
        {
            get
            {
                Vector3 v = transform.TransformDirection(Vector3.forward);
                v.y = 0;
                return v.normalized;
            }
            set
            {
                transform.rotation = Quaternion.LookRotation(value, Vector3.up);
            }
        }

        /// <summary>
        /// The collision radius of the player, in world units.
        /// </summary>
        public float _radius = 0.2f;

        /// <summary>
        /// The Transform to use as center of the player's mass and collition.
        /// This is often the root bone of the rig.
        /// </summary>
        public Transform barycenter;
        /// <summary>
        /// The position of the player's center of mass and collision, in world space.
        /// </summary>
        public Vector3 GetBarycenter()
        {
            return barycenter.position;
        }

        /// <summary>
        /// The collider attached to the same game object as this component.
        /// </summary>
        private Collider _collider;

        /// <summary>
        /// A flag that is true if the character currently has their foot on the ground.
        /// </summary>
        public bool _on_ground = false;

        /// <summary>
        /// The fall speed of the character, applied every frame they are not on the ground.
        /// </summary>
        public float _fall_speed = 65;
        
        private void Awake()
        {
            // Sets the main instance of the player character to this instance.
            main = this;

        }
        
        void Start()
        {
            _collider = GetComponent<Collider>();
            _jumping = false;
            _footstep_clip.LoadAudioData();
            _audio_source.clip = _footstep_clip;
        }

        void Update()
        {
            UpdateMovements();
            UpdateGravity();
            UpdateEntity();
            UpdateCharacter();
            ApplyMotion();
        }

        private void LateUpdate()
        {
            LateUpdateCharacter();
        }

        //###############################################################################################################

        // Movements

        //###############################################################################################################

        /// <summary>
        /// Based on the input, returns a normalized movement direction.
        /// </summary>
        /// <returns>A normalized vector or a zero vector according to the input.</returns>
        Vector3 GetMovementDirection()
        {
            Vector3 r = Vector3.zero;
            if (Input.GetKey(KeyCode.W)) r += Vector3.forward;
            if (Input.GetKey(KeyCode.A)) r += Vector3.left;
            if (Input.GetKey(KeyCode.S)) r += Vector3.back;
            if (Input.GetKey(KeyCode.D)) r += Vector3.right;
            r.Normalize();
            return r;
        }

        /// <summary>
        /// Computes the climbind direction, based on the input direction and the camera's rotation.
        /// </summary>
        Vector3 GetClimbingDirection()
        {
            Vector3 r = GetMovementDirection();
            float z = r.z;
            r.z = 0;
            Vector3 cr = CameraRelativeMovement(r);
            r = new Vector3(cr.x, z, cr.z);
            return r;
        }

        /// <summary>
        /// Transforms a movement direction to be relative to the camera's rotation, instead of absolute.
        /// </summary>
        Vector3 CameraRelativeMovement(Vector3 input)
        {
            Vector3 r = Camera.main.transform.TransformVector(input);
            r.y = 0;
            r.Normalize();
            return r;
        }

        /// <summary>
        /// A flag that is true when the character is in its jumping state.
        /// </summary>
        public bool _jumping;
        /// <summary>
        /// The computed position of the jump's start, in world space.
        /// </summary>
        private Vector3 _jump_start;
        /// <summary>
        /// The computed position of the jump's highest point, in world space.
        /// </summary>
        private Vector3 _jump_peak;
        /// <summary>
        /// The computed position of the jump's end, in world space.
        /// </summary>
        private Vector3 _jump_end;
        /// <summary>
        /// The maximum distance along the XZ plane that the character can jump (how far).
        /// </summary>
        public float _jump_distance = 2;
        /// <summary>
        /// The highest height the character can jump on the Y axis.
        /// </summary>
        public float _jump_height = 1;
        /// <summary>
        /// How fast the character will reach the end of the jump.
        /// </summary>
        public float _jump_speed = 1;
        /// <summary>
        /// A timer used to count how long the character has been in the jump state.
        /// </summary>
        private float _jump_timer;
        /// <summary>
        /// How long to wait before the jump actually starts (an animation plays during this time).
        /// </summary>
        public float _jump_prepare_time = 2;
        /// <summary>
        /// Timer used to count how long the character has been preparing for the jump (animation).
        /// </summary>
        private float _jump_prepare_timer;
        /// <summary>
        /// A flag used to differentiate if the jump is started while the character is not moving and is in idle state.
        /// </summary>
        private bool _idle_jump;

        /// <summary>
        /// Updates the jump state (or starts it if the input is given and the character is on ground).
        /// </summary>
        void UpdateJump()
        {
            if (_jumping)
            {
                _leg_left._forced = false;
                _leg_right._forced = false;
                if (_idle_jump && _jump_prepare_timer < _jump_prepare_time)
                {
                    _jump_prepare_timer += Time.deltaTime;
                    _current_pose = "jump_prepare";
                    return;
                }
                Vector3 start_to_peak = _jump_peak - _jump_start;
                Vector3 peak_to_end = _jump_end - _jump_peak;
                if (_jump_timer >= _jump_speed)
                {
                    _jumping = false;
                    return;
                }
                _jump_timer += Time.deltaTime;
                float t = _jump_timer / _jump_speed;
                float rt = 1 - t;
                Vector3 v = rt * rt * _jump_start + 2 * rt * t * _jump_peak + t * t * _jump_end;
                Vector3 m = v - position;
                if (!CanMoveTo(v))
                {
                    _jumping = false;
                    m = m.normalized * (Vector3.Distance(_collision_point, _collision_hit.point) - _radius);
                }
                position += m;
                if (_idle_jump)
                {
                    _current_pose = "jump_idle";

                }
                else
                {
                    _current_pose = "jump";

                }
                if (t > 0.5f) _can_grab = true;
            }
            else if (_on_ground && Input.GetKeyDown(KeyCode.Space))
            {
                Vector3 v = GetMovementDirection();
                if (v.magnitude == 0)
                {
                    _idle_jump = true;
                    _jump_start = position;
                    _jump_peak = _jump_start + Vector3.up * _jump_height;
                    _jump_end = _jump_start;
                    _jumping = true;
                    _jump_timer = 0;
                    _jump_prepare_timer = 0;
                }
                else
                {
                    v = direction;
                    _idle_jump = false;
                    _jump_start = position;
                    _jump_peak = _jump_start + v.normalized * _jump_distance * 0.5f + Vector3.up * _jump_height;
                    _jump_end = _jump_start + v.normalized * _jump_distance;
                    _jumping = true;
                    _jump_timer = 0;
                }
            }
        }

        /// <summary>
        /// A flag that is only true when the character can move (walk or run).
        /// This can be set to false during cutscenes.
        /// </summary>
        public bool _can_move = true;
        /// <summary>
        /// The speed at which the character moves when moving in walk state.
        /// </summary>
        public float _walking_speed = 3f;
        /// <summary>
        /// The speed at which the character moves when moving in run state.
        /// </summary>
        public float _running_speed = 5f;
        /// <summary>
        /// The speed at which the character turns when moving.
        /// </summary>
        public float _turn_speed = 0.1f;

        /// <summary>
        /// Updates the movement states (walk, run, jump or grab).
        /// </summary>
        void UpdateMovements()
        {
            _current_pose = "idle";
            _collision_point = Vector3.zero;
            _motion = Vector3.zero;
            if (!_can_move) return;
            UpdateJump();
            UpdateGrab();
            if (_on_ground && !_jumping && !_is_grabbing)
            {
                Vector3 move_dir = GetMovementDirection();
                if (move_dir.magnitude == 0) return;
                move_dir = CameraRelativeMovement(move_dir);
                if (move_dir.magnitude == 0) return;
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    _current_pose = "run";
                    Move(move_dir, _running_speed);
                } else
                {
                    _current_pose = "walk";
                    Move(move_dir, _walking_speed);

                }
                Rotate(move_dir, _turn_speed);
            }
        }

        /// <summary>
        /// The motion of the character during the current frame.
        /// </summary>
        public Vector3 _motion = Vector3.zero;

        /// <summary>
        /// Applies a movement to the character.
        /// </summary>
        /// <param name="dir">The direction of the movement, normalized.</param>
        /// <param name="speed">The speed of the movement.</param>
        void Move(Vector3 dir, float speed)
        {
            _motion += dir * speed;
            
        }

        /// <summary>
        /// The current collision point of the character.
        /// </summary>
        Vector3 _collision_point;
        /// <summary>
        /// The current collision info of the character.
        /// </summary>
        RaycastHit _collision_hit;

        // The targets of each major limb, which are tested for collision each frame.
        public Transform _bone_foot_l;
        public Transform _bone_foot_r;
        public Transform _bone_hips;
        public Transform _bone_neck;
        public Transform _bone_leg_l;
        public Transform _bone_leg_r;

        /// <summary>
        /// Tests for collision from the current position to the given target position.
        /// Returns true if the movement is possible, false otherwise (and set the collision info and point).
        /// </summary>
        /// <param name="pos">The target position.</param>
        /// <returns>True if the movement is possible.</returns>
        bool CanMoveTo(Vector3 pos)
        {
            Vector3 m = pos - position;
            // Each major limb is tested for collision.
            List<Vector3> list = new List<Vector3>() { _bone_hips.position,
                _bone_neck.position,
                _bone_leg_l.position,
                _bone_leg_r.position
            };
            foreach (Vector3 p in list)
            {
                Ray r = new Ray(p, m.normalized);
                if (Physics.Raycast(r, out _collision_hit, m.magnitude + _radius))
                {
                    _collision_point = p;
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Applies the current motion of the character to the frame.
        /// </summary>
        void ApplyMotion()
        {
            Vector3 m = _motion * Time.deltaTime;
            if (!CanMoveTo(position + m))
            {
                Vector3 to_hit = _collision_hit.point - _collision_point;
                m = m.normalized * (to_hit.magnitude - _radius);
            }
            position += m;
        }

        /// <summary>
        /// Rotates the character towards the given direction and at the given speed for this frame.
        /// </summary>
        /// <param name="dir">The direction to turn to.</param>
        /// <param name="speed">The speed at which to turn to.</param>
        void Rotate(Vector3 dir, float speed)
        {
            Quaternion old_rot = transform.rotation;
            Quaternion new_rot = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = Quaternion.Slerp(old_rot, new_rot, speed * Time.deltaTime);
        }

        /// <summary>
        /// A timer to know how long the character has been in the air for.
        /// </summary>
        float _in_air_timer = 0;
        /// <summary>
        /// How long the character must be in the air to be considered in the "Falling" state.
        /// </summary>
        public float _fall_air_time = 1;
        /// <summary>
        /// A flag representing if the character is frozen for a moment to let a recovering animation play.
        /// 0: no animation | 1: short animation | 2: long animation.
        /// </summary>
        int _recovering_from_fall_state = 0;
        /// <summary>
        /// A timer to know how long the character has been frozen during the recovering animation.
        /// </summary>
        float _fall_recover_timer;
        /// <summary>
        /// How long the character must be frozen for during the recovering animation.
        /// It is multiplied by the recovering state. The higher, the longer.
        /// </summary>
        public float _fall_recover_time = 1;

        /// <summary>
        /// Updates the falling states if the character is not on the ground, as well as the fall recovery states.
        /// </summary>
        void UpdateGravity()
        {
            GetGround();
            if (_is_grabbing) return;
            if (_jumping)
            {
                _in_air_timer += Time.deltaTime;
                return;
            }
            if (_on_ground)
            {
                _can_grab = false;
                position = _ground_point;
                if (_in_air_timer >= _fall_air_time)
                {
                    _recovering_from_fall_state = 1;
                    if (GetMovementDirection().magnitude > 0) _recovering_from_fall_state = 2;
                    
                }
                if (_recovering_from_fall_state > 0)
                {
                    _can_move = false;
                    _current_pose = "hit_ground_0";
                    if (_recovering_from_fall_state > 1) _current_pose = "hit_ground_1";
                    if (_fall_recover_timer < _fall_recover_time)
                    {
                        _fall_recover_timer += Time.deltaTime;
                    } else
                    {
                        _fall_recover_timer = 0;
                        _recovering_from_fall_state = 0;
                        _can_move = true;
                    }
                }
                _in_air_timer = 0;
            } else 
            {
                _leg_left._forced = false;
                _leg_right._forced = false;
                _can_grab = false;
                if (Input.GetKey(KeyCode.Space))
                    _can_grab = true;
                transform.position += Vector3.down * _fall_speed * Time.deltaTime;
                _current_pose = "fall";
                _in_air_timer += Time.deltaTime;
            }

        }

        // The current ground info detected bellow the character.
        Vector3 _ground_normal = Vector3.up;
        Vector3 _ground_point = Vector3.zero;

        /// <summary>
        /// Computes the height difference between the character's current position and a given position "p".
        /// </summary>
        /// <param name="p">The position to compare against.</param>
        /// <returns>A positive float equal to the height difference of the 2 positions.</returns>
        float HeightDifference(Vector3 p)
        {
            float height=0;
            float center_height = p.y;
            if (center_height > position.y) height = Mathf.Abs(center_height) - Mathf.Abs(position.y);
            else height = Mathf.Abs(position.y) - Mathf.Abs(center_height);
            return height;
        }

        /// <summary>
        /// Retrieves the ground information bellow the character for this frame using raycasts.
        /// </summary>
        void GetGround()
        {
            Ray r = new Ray(GetBarycenter(), Vector3.down);
            RaycastHit hit;
            if (Physics.Raycast(r, out hit, Vector3.Distance(GetBarycenter(), position) + 0.1f))
            {
                _on_ground = true;
                _ground_normal = hit.normal;
                _ground_point = hit.point;
            } else
            {
                _on_ground = false;
            }
        }

        /// <summary>
        /// A flag that is true when the character is in a state where they can grab a ledge or start climbing a surface.
        /// </summary>
        public bool _can_grab = false;
        /// <summary>
        /// A flag that is true when the character is currently climbing.
        /// </summary>
        public bool _is_grabbing = false;

        // The IK of the arms and legs, controlled by script during climbing.
        // They also determine if the character can grab certain points or not, depending on their length.
        public IK _arm_left;
        public IK _arm_right;
        public IK _leg_left;
        public IK _leg_right;

        // The info of the different points "gabbed" by each limb.
        GripInfo _hand_left_target;
        GripInfo _hand_right_target;

        GripInfo _foot_left_target;
        GripInfo _foot_right_target;

        /// <summary>
        /// Updates the climbing state and the transition to it.
        /// </summary>
        void UpdateGrab()
        {
            if (!_can_grab && !_is_grabbing) return;
            if (_can_grab && !_is_grabbing)
            {
                // Find out if there's a surface to grab.
                //Debug.Log("Looking for something to grab...");
                List<GripInfo> grips = Grip.GetAllPoints(Vector3.forward, GetBarycenter(), 0.5f);
                if (grips.Count > 0)
                {
                    // If so, enter the climbing state.
                    //Debug.Log("Can Grab Something!");
                    _current_grab_pos = GetBarycenter();
                    grips.Sort(SortGripsByDistance);
                    _hand_left_target = grips[0];
                    _arm_left._forced_target = _hand_left_target.position + _hand_left_target.left * 0.05f;
                    _hand_right_target = grips[0];
                    _arm_right._forced_target = _hand_right_target.position + _hand_right_target.right * 0.05f;
                    _foot_left_target = FindFootWallGrip(_leg_left);
                    _foot_right_target = FindFootWallGrip(_leg_right);
                    _leg_left._forced_target = _foot_left_target.position + _foot_left_target.left * 0.05f;
                    _leg_right._forced_target = _foot_right_target.position + _foot_right_target.left * 0.05f;
                    _is_grabbing = true;
                    _jumping = false;
                    _on_ground = false;
                }
            }
            if (_is_grabbing)
            {
                // Update the climbing state.
                _current_pose = "grab_idle";
                Climb();
                _arm_left._forced = true;
                _arm_right._forced = true;
                _leg_left._forced = true;
                _leg_right._forced = true;
                RefreshGrabPosition();
                RefreshGrabDirection();
                if (Input.GetKeyDown(KeyCode.LeftShift))
                {
                    // Let go of the surface and fall.
                    position -= direction.normalized * _radius;
                    _is_grabbing = false;
                    _arm_left._forced = false;
                    _arm_right._forced = false;
                    _leg_left._forced = false;
                    _leg_right._forced = false;
                } else if (Input.GetKeyDown(KeyCode.Space))
                {
                    // Jump vertically from the current climbing position.
                    _idle_jump = true;
                    position -= direction.normalized * _radius;
                    _jump_start = (position+GetBarycenter())*0.5f;
                    _jump_peak = _jump_start + Vector3.up * _jump_height * 0.5f;
                    _jump_end = _jump_start+direction*0.5f;
                    _jumping = true;
                    _jump_timer = 0;
                    _jump_prepare_timer = _jump_prepare_time;
                    _is_grabbing = false;
                    _arm_left._forced = false;
                    _arm_right._forced = false;
                    _leg_left._forced = false;
                    _leg_right._forced = false;
                    _can_grab = false;
                    // Need to add a cooldown here as it is possible to exploit this to climb way too fast.
                }
            }
        }

        /// <summary>
        /// A flag from 0 to 3 which determines the current state of the hands in climbing:
        /// 0: find a grip for the left hand. 1: move the left hand to the grip. 2: find a grip for the right hand. 3: move the right hand to find a grip.
        /// </summary>
        public int _climb_step_hands = 0;
        /// <summary>
        /// The speed at which the hands move from one grip point to another.
        /// </summary>
        public float _climb_speed_hands = 1.7f;
        /// <summary>
        /// A flag from 0 to 3 which determines the current state of the feet in climbing:
        /// 0: find a grip for the left foot. 1: move the left foot to the grip. 2: find a grip for the right foot. 3: move the right foot to find a grip.
        /// </summary>
        public int _climb_step_feet = 0;
        public float _climb_speed_feet = 2.3f;

        void Climb()
        {
            //Hands
            if (_climb_step_hands == 0) // left hand - find grip
            {
                GripInfo next = GetNextGrip(_hand_left_target, _arm_left);
                if (next != null)
                {
                    _hand_left_target = next;
                    _climb_step_hands++;
                }
                else
                    _climb_step_hands += 2;
            }
            else if (_climb_step_hands == 1) // left hand - move to grip
            {
                if (MoveIKToGrip(_arm_left, _hand_left_target.position + _hand_left_target.left * 0.05f, _climb_speed_hands))
                    _climb_step_hands++;
            }
            else if (_climb_step_hands == 2) // right hand - find grip
            {

                GripInfo next = GetNextGrip(_hand_right_target, _arm_right);
                if (next != null)
                {
                    _hand_right_target = next;
                    _climb_step_hands++;
                }
                else
                    _climb_step_hands += 2;
            }
            else if (_climb_step_hands == 3) // right hand - move to grip
            {
                if (MoveIKToGrip(_arm_right, _hand_right_target.position + _hand_right_target.right * 0.05f, _climb_speed_hands))
                    _climb_step_hands++;
            }
            else
            {
                _climb_step_hands = 0;
            }

            //Feet
            if (_climb_step_feet == 0) // left foot - find grip
            {

                GripInfo next = FindFootWallGrip(_leg_left);
                if (next != null)
                {
                    _foot_left_target = next;
                    _climb_step_feet++;
                }

            }
            else if (_climb_step_feet == 1) // left foot - move to grip
            {
                if (MoveIKToGrip(_leg_left, _foot_left_target.position + _foot_left_target.left * 0.05f, _climb_speed_feet))
                    _climb_step_feet++;
            }
            else if (_climb_step_feet == 2) // right foot - find grip
            {

                GripInfo next = FindFootWallGrip(_leg_right);
                if (next != null)
                {
                    _foot_right_target = next;
                    _climb_step_feet++;
                }
            }
            else if (_climb_step_feet == 3) // right foot - move to grip
            {
                if (MoveIKToGrip(_leg_right, _foot_right_target.position + _foot_right_target.right * 0.05f, _climb_speed_feet))
                    _climb_step_feet++;
            }
            else
            {
                _climb_step_feet = 0;
            }
        }

        /// <summary>
        /// Unused for now as the feet mostly follow the rest of the body during climbing in the scenarios of the demo.
        /// Could be re-implemented in futur levels.
        /// Computes which grip point to go to next, if any, using the current list of available grip points.
        /// The points are sorted by proximity and angle to be sure to move in the direction given by the player.
        /// </summary>
        /// <param name="current">The current grip.</param>
        /// <param name="ik">The IK to test.</param>
        /// <returns>The new grip for this IK.</returns>
        GripInfo GetNextFootGrip(GripInfo current, IK ik)
        {
            Vector3 input_dir = GetClimbingDirection();
            if (input_dir.magnitude == 0) return null;
            Vector3 base_pos = ik.GetBones()[1]._head;
            float reach = ik.GetBones()[1]._length;
            Debug.DrawLine(base_pos, base_pos + input_dir * reach);

            List<GripInfo> grips = new List<GripInfo>();
            grips.AddRange(Grip.GetAllPoints(input_dir, base_pos, reach + 0.1f));
            if (grips.Count == 0) return null;
            _current_grab_pos = current.position;
            _current_grab_dir = input_dir;
            grips.Sort(SortGripsByDistanceAndAngle);
            for (int i = 0; i < grips.Count; i++)
            {
                float dist = Vector3.Distance(grips[i].position, current.position);
                float angle = Vector3.Angle(grips[i].position - current.position, input_dir);
                if (dist > _climb_speed_hands * Time.deltaTime && angle < 60)
                    return grips[i];
            }
            return null;
        }

        /// <summary>
        /// Computes a grip point for a foot IK by performing raycasts on the surface climbed, then snapping the grip point onto it.
        /// </summary>
        /// <param name="ik">The IK to test.</param>
        /// <returns>The new grip for this IK.</returns>
        GripInfo FindFootWallGrip(IK ik)
        {
            Vector3 mov = GetMovementDirection();
            Vector3 input_dir = GetClimbingDirection().normalized;
            if (ik == _leg_left && (mov.x > 0||mov.z<0))
            {
                input_dir = Vector3.zero;
            } else if (ik == _leg_right && (mov.x < 0 || mov.z > 0))
            {
                input_dir = Vector3.zero;
            }
            Vector3 pos = ik._target_transform.position + input_dir * ik.total_length * 0.5f;
            Vector3 dir = -direction;
            Ray r = new Ray(pos + dir.normalized * 0.2f, -dir);
            RaycastHit hit;
            if (Physics.Raycast(r, out hit, 1.8f))
            {
                return new GripInfo(hit.point, hit.normal);   
            } else
            {
                return new GripInfo(ik._origin - Vector3.down * ik.total_length, dir);
            }
        }

        /// <summary>
        /// Moves an IK to the given position at the given speed, and returns true if it reached the destination.
        /// </summary>
        /// <param name="ik">The IK to move.</param>
        /// <param name="pos">The target position.</param>
        /// <param name="speed">The speed at which the IK moves.</param>
        /// <returns>True if the position was reached, and false otherwise.</returns>
        bool MoveIKToGrip(IK ik, Vector3 pos, float speed)
        {
            float dist = Vector3.Distance(ik._forced_target, pos);
            if (dist <= _climb_speed_hands * Time.deltaTime)
            {
                ik._forced_target = pos;
                return true;
            }
            Vector3 v = pos - ik._forced_target;
            ik._forced_target += v.normalized * speed * Time.deltaTime;
            return false;
        }

        /// <summary>
        /// Computes which grip point to go to next, if any, using the current list of available grip points.
        /// The points are sorted by proximity and angle to be sure to move in the direction given by the player.
        /// </summary>
        /// <param name="current">The current grip.</param>
        /// <param name="ik">The IK to test.</param>
        /// <returns>The new grip for this IK.</returns>
        GripInfo GetNextGrip(GripInfo current, IK ik)
        {
            Vector3 input_dir = GetClimbingDirection().normalized;
            if (input_dir.magnitude == 0) return null;
            Vector3 base_pos = ik._origin;
            float reach = ik.total_length;
            Debug.DrawLine(base_pos, base_pos + input_dir * reach);

            List<GripInfo> grips = new List<GripInfo>();
            grips.AddRange(Grip.GetAllPoints(input_dir, base_pos, reach + 0.1f));
            if (grips.Count == 0) return null;
            _current_grab_pos = current.position;
            _current_grab_dir = input_dir;
            grips.Sort(SortGripsByDistanceAndAngle);
            for (int i=0;i < grips.Count; i++)
            {
                float dist = Vector3.Distance(grips[i].position, current.position);
                float angle = Vector3.Angle(grips[i].position - current.position, input_dir);
                if (dist > _climb_speed_hands * Time.deltaTime && angle < 60)
                    return grips[i];
            }
            return null;
        }

        /// <summary>
        /// Computes the movement that results from the climbing state this frame, and applies it to the character's position.
        /// </summary>
        void RefreshGrabPosition()
        {
            Vector3 barycenter = (_arm_left._target
                + _arm_right._target) / 2.0f;
            Vector3 v = barycenter - GetBarycenter();
            position += v;
        }


        /// <summary>
        /// Computes the rotation that resuts from the climbing state this frame, and applies it to the character's rotation.
        /// </summary>
        void RefreshGrabDirection()
        {
            Vector3 dir = _hand_left_target.normal + _hand_right_target.normal;
            dir = -dir.normalized;
            direction = Vector3.Lerp(direction, dir, 0.2f);
        }

        // Current grabbing positions and direction used in sorting grip points.
        // These are sets in the GetNextGrip methods.
        Vector3 _current_grab_pos;
        Vector3 _current_grab_dir;

        int SortGripsByDistance(GripInfo p1, GripInfo p2)
        {
            float d1 = Vector3.Distance(_current_grab_pos, p1.position);
            float d2 = Vector3.Distance(_current_grab_pos, p2.position);
            if (d1 < d2) return 1;
            else if (d1 == d2) return 0;
            else return -1;
        }

        int SortGripsByDistanceAndAngle(GripInfo p1, GripInfo p2)
        {
            float d1 = Vector3.Distance(_current_grab_pos, p1.position);
            float d2 = Vector3.Distance(_current_grab_pos, p2.position);
            float a1 = Vector3.Angle(_current_grab_dir, p1.position - _current_grab_pos);
            float a2 = Vector3.Angle(_current_grab_dir, p1.position - _current_grab_pos);
            if (d1 < d2 || a1 < a2) return 1;
            else return -1;
        }

        //###############################################################################################################

        // Character, Animation and Audio feedbacks.

        //###############################################################################################################

        /// <summary>
        /// The character component linked to the player.
        /// </summary>
        public Character _character;

        /// <summary>
        /// The name of the current animation of the character.
        /// </summary>
        public string _current_pose = "idle";

        /// <summary>
        /// The clip to play each footstep of the character.
        /// </summary>
        public AudioClip _footstep_clip;

        /// <summary>
        /// The audio source that plays various sounds of the character, like footsteps.
        /// </summary>
        public AudioSource _audio_source;

        /// <summary>
        /// Updates the character animations and states.
        /// </summary>
        public void UpdateCharacter()
        {
            Hurt();
            _character.Pose(_current_pose);
        }

        /// <summary>
        /// Makes sure the character has both feet on the ground.
        /// </summary>
        public void LateUpdateCharacter()
        {
            FeetOnGround(_leg_left);
            FeetOnGround(_leg_right);
        }


        /// <summary>
        /// Performs IK solving on the legs when on the ground state to snap the feet to the ground.
        /// </summary>
        /// <param name="ik">The Leg IK to transform.</param>
        void FeetOnGround(IK ik)
        {
            if (_on_ground)
            {
                Vector3 se = ik._target_transform.position - ik._origin;
                Ray ray = new Ray(ik._origin, se);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, ik.total_length))
                {
                    ik._forced = true;
                    ik._forced_target = hit.point + Vector3.up * 0.1f;
                } else
                {
                    ik._forced = false;
                }
            }
        }

        /// <summary>
        /// This flag is true when the character is was hit by an attack, and until its cooldown is finished.
        /// </summary>
        public bool _hurt = false;

        /// <summary>
        /// Which side of the character was hit by the attack. This changes the animation to play.
        /// 0: left, 1: right, 2: back.
        /// </summary>
        int _hurt_side = 0;

        /// <summary>
        /// The different cooldowns to apply when being hurt by an attack, depending on the side hit.
        /// </summary>
        float[] _hurt_time = new float[] { 0.5f, 0.5f, 0.5f };

        /// <summary>
        /// A timer that counst how long the character has been in the hurt state.
        /// </summary>
        float _hurt_timer = 0;

        /// <summary>
        /// The names of the different hit animations, by side.
        /// </summary>
        string[] _hurt_pose = new string[] { "hit_left", "hit_right", "hit_back" };

        /// <summary>
        /// Callback event when the character is hit by an attack.
        /// </summary>
        /// <param name="side">The id of the side hit.</param>
        public void OnHit(int side)
        {
            if (!_hurt)
            {
                _hurt = true;
                _hurt_side = side;
                _character.PlaySound("hurt_0");
            }
        }

        /// <summary>
        /// Updates the Hurt state of the character if it was hit by an attack.
        /// </summary>
        void Hurt()
        {
            if (_hurt)
            {
                _can_grab = false;
                _can_move = false;
                _hurt_timer += Time.deltaTime;
                if (_hurt_timer >= _hurt_time[_hurt_side])
                {
                    _hurt = false;
                    _can_move = true;
                    _hurt_timer = 0;
                } else
                {
                    _current_pose = _hurt_pose[_hurt_side];
                }
            }
        }

        //###############################################################################################################

        // Entity (Attacks & Health)

        //###############################################################################################################

        /// <summary>
        /// The entity component linked to this character.
        /// </summary>
        public Entity_Player entity;

        /// <summary>
        /// A transform representing where to attach a weapon mesh to the character's rig.
        /// </summary>
        public Transform _weapon_bone;

        /// <summary>
        /// Updates the entity's states.
        /// </summary>
        public void UpdateEntity()
        {
            entity.player = this;
            RefreshWeapon();
            RefreshAttack();
        }

        /// <summary>
        /// Updates the entity's weapon transforms to match the rig's.
        /// </summary>
        void RefreshWeapon()
        {
            if (entity.weapon != null)
            {
                entity.weapon.transform.position = _weapon_bone.position;
                entity.weapon.transform.rotation = _weapon_bone.rotation;
            }
        }

        /// <summary>
        /// A flag that is true when the character is currently attacking.
        /// </summary>
        bool _attacking = false;

        /// <summary>
        /// An index representing the current combo attack performed by the player.
        /// It can only go as far as max combo.
        /// Attacks with a higher combo deal more damage.
        /// </summary>
        int _attack_combo = 0;
        /// <summary>
        /// The maximum number of combos allowed.
        /// </summary>
        int _max_combo = 1;
        /// <summary>
        /// How long the current combo has been running.
        /// </summary>
        float _combo_timer = 0;
        /// <summary>
        /// The minimum time an attack must last before the next combo can be triggered if the attack input is received again.
        /// </summary>
        float _combo_time = 0.2f;
        /// <summary>
        /// A timer that counts how long an attack has lasted.
        /// </summary>
        float _attack_timer = 0;
        /// <summary>
        /// The list of the maximum duration of each attack in the combo.
        /// </summary>
        float[] _attacks_time = new float[] { 0.5f };
        /// <summary>
        /// The list of the animation names of each attack in the combo.
        /// </summary>
        string[] _attacks_pose = new string[] { "attack_0" };

        /// <summary>
        /// Updates the attacking state and combos.
        /// </summary>
        void RefreshAttack()
        {
            if (entity.weapon != null)
            {
                if (_attacking)
                {
                    _current_pose = _attacks_pose[_attack_combo];
                    if (_combo_timer > 0)
                    {
                        _combo_timer -= Time.deltaTime;
                        if (Input.GetMouseButtonDown(0))
                        {
                            _attack_combo++;
                        } else if (_combo_timer <= 0)
                        {
                            _attacking = false;
                            _attack_combo = 0;
                            _combo_timer = 0;
                            _can_move = true;
                        }
                        if (_attack_combo >= _max_combo) _attack_combo = 0;
                    } else
                    {
                        if (_attack_timer >= _attacks_time[_attack_combo] - _combo_time)
                        {
                            _attack_timer = 0;
                            _combo_timer = _combo_time;
                        }
                        entity.Attack(_attack_combo, _attack_timer);
                        _attack_timer += Time.deltaTime;
                    }

                } else
                {
                    if (_on_ground && !_jumping && !_is_grabbing && !_hurt)
                    {
                        if (Input.GetMouseButtonDown(0))
                        {
                            _attacking = true;
                            _can_move = false;
                        }
                    }
                }
            }
        }

    }
}