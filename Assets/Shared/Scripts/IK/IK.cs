using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A runtime class representing a Bone segment of the IK structure.
/// It is used to perform local computations and transforms on the Bone,
/// as well as store data on them (its <c>length</c>, for instance).
/// </summary>
public class IK_Bone
{
    /// <summary>
    /// The Transform associated with the Bone's head.
    /// Used to apply the result of the IK computation to the Rig.
    /// </summary>
    public Transform _head_transform;
    /// <summary>
    /// The Transform associated with the Bone's tail.
    /// </summary>
    public Transform _tail_transform;
    /// <summary>
    /// The start local position of the head.
    /// Used each time the IK is computed, as a starting point.
    /// </summary>
    public Vector3 _head_base;
    /// <summary>
    /// The start local position of the tail.
    /// Used each time the IK is computed, as a starting point.
    /// </summary>
    public Vector3 _tail_base;
    /// <summary>
    /// The rotation of the Bone segment at the start.
    /// Used each time the IK is computed, as a starting point.
    /// </summary>
    public Quaternion _roll_base;
    /// <summary>
    /// The length of the Bone segment. Computed at start.
    /// Used each time the IK is computed, as a limit.
    /// </summary>
    public float _length;
    /// <summary>
    /// The current position of the head, computed and set by the IK.
    /// </summary>
    public Vector3 _head;
    /// <summary>
    /// The current position of the tail, computed and set by the IK.
    /// </summary>
    public Vector3 _tail;

    /// <summary>
    /// Returns or sets the current rotation of the Bone.
    /// Avoid setting this manually. Instead, use the <c>Roll()</c> method.
    /// </summary>
    public Quaternion _roll
    {
        get
        {
            return _head_transform.rotation;
        } set
        {
            _head_transform.rotation = value;
        }
    }

    /// <summary>
    /// Creates an IK Bone instance from the Transforms of 2 GameObjects of the Rig of a character. 
    /// These 2 Transforms represent 1 Bone segment of the IK.
    /// </summary>
    /// <param name="t1">The Transform representing the "head" of the Bone.</param>
    /// <param name="t2">The Transform representing the "tail" of the Bone.</param>
    public IK_Bone(Transform t1, Transform t2)
    {
        // Store the start positions and rotation.
        _head_base = t1.position;
        _tail_base = t2.position;
        _roll_base = t1.rotation;

        _head_transform = t1;
        _tail_transform = t2;

        // Compute the length of the Bone.
        _length = Vector3.Distance(t1.position, t2.position);
        
    }

    /// <summary>
    /// Computes in which direction the Bone is supposed to turn, according to an orbit position "o".
    /// The orbit position indicates in which direction the IK will bend.
    /// This prevents an arm to bend backwards, for example.
    /// </summary>
    /// <param name="o">The position of the orbit, in world space.</param>
    public void Roll(Vector3 o)
    {
        // Create a plane using the tail, head and orbit positions.
        Vector3 to_tail = _tail - _head;
        Vector3 to_o = o - _head;
        Plane p = new Plane(_tail, _head, o);

        // Then find out which side of the plane the current transformation points to.
        Vector3 dir = Vector3.Cross(to_tail, p.normal).normalized;
        float a = Vector3.Angle(dir, to_o);

        // If this is the wrong side of the plane, revert the direction of the Bone.
        if (a > 90) dir = -dir;
        _roll = Quaternion.LookRotation(-dir.normalized, to_tail.normalized);
    }
}

/// <summary>
/// A component representing a continuous segment of Bones, which will perform Inverse Kinematics every frame.
/// This allows the segment to bend reallistically to reach a target position, simulating an arm or legs.
/// This IK implementation uses actual IK computations. Any number of Bones in a segment can be used.
/// This component does not have to be attached to the bone segment itself.
/// However, attaching it to the segment's root bone is good practice.
/// </summary>
[AddComponentMenu("Physics/IK")]
public class IK : MonoBehaviour
{
    /// <summary>
    /// The bones representing the segment affected by IK.
    /// They must be in order, from the first bone's head (root) to the last bone's tail (leaf).
    /// </summary>
    public List<Transform> _transforms = new List<Transform>();

    /// <summary>
    /// A runtime list of <c>IK_Bone</c> objects, created from the <c>_transforms</c> list.
    /// </summary>
    private List<IK_Bone> _bones;
    /// <summary>
    /// The runtime list of <c>IK_Bones</c> of this IK.
    /// </summary>
    /// <returns>A list of <c>IK_Bones</c>, from root to leaf.</returns>
    public List<IK_Bone> GetBones()
    {
        return _bones;
    }

    /// <summary>
    /// Tells the IK to follow a manual target instead of its normal target position (represented by a Transform's position).
    /// </summary>
    public bool _forced=false;
    /// <summary>
    /// A manually set target to follow. The field <c>_forced</c> must be set to true in order to follow this target instead of <c>_target_transform</c>.
    /// </summary>
    public Vector3 _forced_target;
    /// <summary>
    /// Returns the current target position the IK will follow and compute towards.
    /// This is the field <c>_forced_target</c> if <c>_forced</c> is true.
    /// Otherwise, it defaults to the assigned <c>_target_transform</c>.
    /// </summary>
    public Vector3 _target {
        get
        {
            if (_forced) return _forced_target;
            else return _target_transform.position;
        }
    }
    /// <summary>
    /// Represents the position that the IK will try to reach each frame, bending its bone towards.
    /// This field must be set to something in the inspector for the IK to work properly.
    /// </summary>
    public Transform _target_transform;
    /// <summary>
    /// Represents a point in space: the side of which the Bones of the IK will bend, instead of bending in any direction.
    /// The Bones will all align on a plane represented by this point, the target transform and the root bone.
    /// </summary>
    public Transform _axis;

    /// <summary>
    /// A factor specifying how accurate the IK computation must be to reach it's target, in world units.
    /// A smaller factor results in more iterations.
    /// </summary>
    [Range(0.01f, 10f)]
    public float _tolerance = 0.1f;
    /// <summary>
    /// The maximum number of iterations allowed.
    /// More iterations are safer but cost performance.
    /// Less iterations may result in missing the target.
    /// </summary>
    [Range(3, 100)]
    public int _max_iter = 10;

    /// <summary>
    /// This flag sets to true when all Bones of the IK have been registered.
    /// </summary>
    private bool _initialized = false;

    /// <summary>
    /// This flag switches the IK computation on if true, otherwise off.
    /// </summary>
    public bool _active = true;


	void Start () {
        Initialize();
        _active = true;
    }

    void Initialize()
    {
        if (_bones == null) _initialized = false;
        else if (_bones.Count == 0) _initialized = false;
        if (!_initialized)
        {
            _bones = new List<IK_Bone>();
            for(int i = 1; i < _transforms.Count; i++)
            {
                Transform t1 = _transforms[i - 1];
                Transform t2 = _transforms[i];
                _bones.Add(new IK_Bone(t1, t2));
            }
            _initialized = true;
        }
    }

    /// <summary>
    /// If something went wrong, this forces a reinitialization of the IK.
    /// </summary>
    public void ForceInit()
    {
        _initialized = false;
        Initialize();
    }

   
    private void FixedUpdate()
    {
        // Prevents some bending artifacts.
        // Do not remove!!!
        SetupBonesForSolving();

    }

    // Does the solving after animations have potentially moved the target positions.
    private void LateUpdate()
    {
        // Initialize if _initialize is false.
        Initialize();
        // Prepares the Bones for computation.
        SetupBonesForSolving();
        // 
        StaticPose();
        // 
        ClampToPlane();
        // 
        Solve();
        //
        Apply();
    }

    /// <summary>
    /// Returns the maximum length of the IK Bone segment.
    /// </summary>
    public float total_length
    {
        get
        {
            float r = 0;
            foreach (IK_Bone b in _bones)
            {
                r += b._length;
            }
            return r;
        }
    }

    /// <summary>
    /// Represents the root of the IK Bone segment.
    /// It is set automatically when calling <c>SetupBonesForSolving()</c>.
    /// </summary>
    public Vector3 _origin;

    /// <summary>
    /// Clamps all bones to a single plane, represented by the root, the target, and the orbit positions.
    /// This avoids each bone to bend freely in any direction.
    /// </summary>
    void ClampToPlane()
    {
        if (!_active) return;
        Plane p = new Plane(_target, _axis.position, _origin);
        for (int i = 0; i < _bones.Count; i++)
        {
            Vector3 t_on_p = p.ClosestPointOnPlane(_bones[i]._tail);
            Vector3 h2t = t_on_p - _bones[i]._head;
            _bones[i]._tail = _bones[i]._head + h2t.normalized * _bones[i]._length;
            if (i < _bones.Count - 1) _bones[i + 1]._head = _bones[i]._tail;
        }
    }

    /// <summary>
    /// Checks if the bones are bending towards or away from the orbit position (<c>_axis</c>).
    /// If they bend away from it, it "flips" the vector <c>v</c> in order that they do.
    /// </summary>
    /// <param name="v">The vector representing the direction of the bending of the bones.</param>
    /// <returns>The same vector, but flipped if the bones were bending away.</returns>
    Vector3 AxisFlip(Vector3 v)
    {
        Plane p = new Plane(_target, _axis.position, _origin);
        Vector3 to_target = _target - _origin;
        Vector3 pn = p.normal;
        Vector3 sn = Vector3.Cross(to_target, pn).normalized;
        Vector3 to_axis = _axis.position - _origin;
        float a = Vector3.Angle(sn.normalized, to_axis.normalized);
        if (a > 90) sn = -sn.normalized;
        Plane s = new Plane(sn, _origin);
        if (s.SameSide(_axis.position, v)) return v;
        Vector3 vs = s.ClosestPointOnPlane(v);
        Vector3 v_to_s = vs - v;
        Vector3 rv = v + v_to_s * 2;
        return rv;
    }

    /// <summary>
    /// Performs a Backwards Kinematik solving iteration on all bones.
    /// </summary>
    void Backward()
    {
        _bones[_bones.Count-1]._tail = _target;
        for (int i = _bones.Count - 1; i >= 0; i--)
        {
            Vector3 to_tail = _bones[i]._tail - _bones[i]._head;
            _bones[i]._head = _bones[i]._head + to_tail.normalized * (to_tail.magnitude - _bones[i]._length);
            _bones[i]._head = AxisFlip(_bones[i]._head);
            if (i>0) _bones[i - 1]._tail = _bones[i]._head;
        }
    }

    /// <summary>
    /// Performs a Forward Kinematik solving iteration on all bones.
    /// </summary>
    void Forward()
    {
        _bones[0]._head = _origin;
        for (int i = 0; i < _bones.Count; i++)
        {
            Vector3 to_head = _bones[i]._head - _bones[i]._tail;
            _bones[i]._tail = _bones[i]._tail + to_head.normalized * (to_head.magnitude - _bones[i]._length);
            _bones[i]._tail = AxisFlip(_bones[i]._tail);
            if (i < _bones.Count-1) _bones[i + 1]._head = _bones[i]._tail;
        }
    }

    /// <summary>
    /// Prepares the IK computation by accuiring the current positions of each Bones.
    /// </summary>
    void SetupBonesForSolving()
    {
        _origin = _bones[0]._head;
        foreach (IK_Bone b in _bones)
        {
            b._head = b._head_transform.position;
            b._tail = b._tail_transform.position;
            //b._roll = b._roll_base;
        }
    }

    /// <summary>
    /// If the IK is not active, rests the position of all bones
    /// to their start position (T Pose or A Pose depending on the rig).
    /// </summary>
    void StaticPose()
    {
        if (!_active)
        {
            foreach (IK_Bone b in _bones)
            {
                b._head = b._head_base;
                b._tail = b._tail_base;
                b._roll = b._roll_base;
            }
        }
    }

    /// <summary>
    /// Performs the solving iterations of the IK.
    /// The solving stops if the distance between the IK's leaf and the target is smaller or equal to the tolerance value.
    /// Otherwise, it stops when the maximum iteration number is reached.
    /// </summary>
    void Solve()
    {
        
        if (!_active) return;

        float dist = Vector3.Distance(_origin, _target);
        float tl = total_length;
        if (dist > tl)
        {
            Vector3 to_target = _target - _origin;
            for (int i = 0; i < _bones.Count; i++)
            {
                _bones[i]._tail = _bones[i]._head + to_target.normalized * _bones[i]._length;
                if (i < _bones.Count - 1) _bones[i + 1]._head = _bones[i]._tail;
                //_bones[i].Roll(_axis.position);
            }
        } else
        {
            int iter = 0;
            float diff = Vector3.Distance(_bones[_bones.Count - 1]._tail, _target);
            while (diff > _tolerance)
            {
                Backward(); 
                Forward();

                diff = Vector3.Distance(_bones[_bones.Count-1]._tail, _target);
                iter++;
                if (iter > _max_iter) break;
            }
        }
    }

    /// <summary>
    /// Detaches each bone from its parent (child: tail, parent: head).
    /// This is needed to apply the result of the IK in world space.
    /// </summary>
    void RemoveParents()
    {
        foreach (IK_Bone b in _bones)
        {
            b._tail_transform.SetParent(null, true);
        }
    }

    /// <summary>
    /// Re-attaches each bone to its parent (child: tail, parent: head).
    /// </summary>
    void RestoreParents()
    {
        foreach (IK_Bone b in _bones)
        {
            b._tail_transform.SetParent(b._head_transform, true);
        }
    }

    /// <summary>
    /// Apply the result of the IK to the Bone transforms.
    /// Since the result is in world space, the bones are detached first, 
    /// then re-attached together after the result is applied.
    /// </summary>
    void Apply()
    {
        RemoveParents();

        Plane p = new Plane(_target, _axis.position, _origin);

        foreach (IK_Bone b in _bones)
        {
            Vector3 h2t = b._tail - b._head;
            //b._head_transform.position = b._head;
            Vector3 fwd = -Vector3.Cross(p.normal, h2t).normalized;
            if (_active)
            {
                b._roll = Quaternion.LookRotation(fwd, h2t);
            }
            b._tail_transform.position = b._tail;
        }

        RestoreParents();
    }

    /// <summary>
    /// Draws a line for each Bone segment in the IK.
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        for (int i = 0; i < _transforms.Count-1; i++)
        {
            Gizmos.DrawLine(_transforms[i].position, _transforms[i + 1].position);
        }      
    }
}
