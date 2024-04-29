using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Logic.Characters;

namespace Game.Logic.Entities
{
    public class Entity_Shadow : Entity
    {
        public Character character;
        string _current_pose;

        public override float max_health
        {
            get
            {
                return 100;
            }
        }
        public override void OnStart()
        {
            base.OnStart();
        }
        public override void OnUpdate()
        {
            UpdateAI();
            UpdateMovements();
            UpdateCharacter();
            base.OnUpdate();
        }

        public override int team
        {
            get
            {
                return 1; // foes
            }
        }
        public override bool can_be_hit
        {
            get
            {
                return true;
            }
        }

        //######################################################################################################

        // Movements

        //######################################################################################################

        public Transform _barycenter;
        public float _run_speed = 5f;
        public float _turn_speed = 1;
        public float _fall_speed = 1;
        public float _radius = 0.2f;
        public float _steep_angle = 30;
        public Transform _bone_foot_l;
        public Transform _bone_foot_r;
        public Transform _bone_hips;
        public Transform _bone_neck;
        public Transform _bone_leg_l;
        public Transform _bone_leg_r;
        public ParticleSystem _particles;

        bool _can_move = true;
        bool _on_ground=false;
        Vector3 _ground_point;
        Vector3 _movement;

        void UpdateMovements()
        {
            Gravity();
            ApplyMovements();
            Collide();
        }

        public override float radius
        {
            get
            {
                return _radius;
            }
        }
        public override bool can_collide
        {
            get
            {
                return true;
            }
        }

        void Rotate(Vector3 dir, float speed)
        {
            Quaternion old_rot = transform.rotation;
            dir.y = 0;
            Quaternion new_rot = Quaternion.LookRotation(dir.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(old_rot, new_rot, speed * Time.deltaTime);
        }

        void Move(Vector3 dir, float speed)
        {
            dir.y = 0;
            _movement += dir.normalized * speed;
        }

        bool CanMoveTo(Vector3 dir)
        {
            List<Vector3> list = new List<Vector3>() { _bone_hips.position,
                _bone_neck.position,
                _bone_leg_l.position,
                _bone_leg_r.position
            };
            foreach (Vector3 p in list)
            {
                Ray ray = new Ray(p, dir);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, dir.magnitude + _radius))
                {
                    Vector3 to_hit = hit.point - position;
                    float angle = Vector3.Angle(to_hit.normalized, direction);
                    if (angle > _steep_angle)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        void ApplyMovements()
        {
            if (_movement.magnitude == 0) return;
            Vector3 dir = _movement * Time.deltaTime;
            if (CanMoveTo(dir))
            {
                position += dir;
            }
            _movement = Vector3.zero;
        }

        void Gravity()
        {
            Ray ray2ground = new Ray(_barycenter.position, Vector3.down);
            RaycastHit hit;
            if (Physics.Raycast(ray2ground, out hit, Vector3.Distance(_barycenter.position, position) + 0.1f))
            {
                _ground_point = hit.point;
                _on_ground = true;
            } else
            {
                _on_ground = false;
            }
            if (!_on_ground)
            {
                position += Vector3.down * _fall_speed * Time.deltaTime;
            } else
            {
                position = _ground_point;
            }
        }



        //######################################################################################################

        // AI

        //######################################################################################################
        
        public float _attack_distance = 1;
        public float _attack_angle = 5;

        bool _attacking = false;
        int _attack_combo = 0;
        int _max_combo = 1;
        float _combo_timer = 0;
        float _combo_time = 0.2f;
        float _attack_timer = 0;
        float[] _attacks_time = new float[] { 1.5f };
        string[] _attacks_pose = new string[] { "attack_0" };

        void UpdateAI()
        {
            if (dead) OnDeath();
            else
            {
                OnSpawn();
                Hurt();
                float dist2player = GetDistanceToPlayer();
                float angle2player = GetAngleToPlayer();
                if (dist2player <= _attack_distance)
                {
                    if (angle2player <= _attack_angle)
                    {
                        AttackPlayer();

                    }
                    else
                    {
                        TurnToPlayer();
                    }
                }
                else
                {
                    MoveToPlayer();
                }
                RefreshAttack();
            }
        }

        float _spawn_time = 2.8f;
        float _spawn_timer = 0;
        bool _spawned = false;
        void OnSpawn()
        {
            if (_spawned == false)
            {
                _spawn_timer += Time.deltaTime;
                if (_spawn_timer >= _spawn_time)
                {
                    _spawned = false;
                    _can_move = true;
                } else
                {
                    _can_move = false;
                    _current_pose = "spawn_0";
                }
            }
        }

        float GetAngleToPlayer()
        {
            Vector3 to_player = Player.main.position - position;
            return Vector3.Angle(to_player, direction);
        }

        float GetDistanceToPlayer()
        {
            return Vector3.Distance(position, Player.main.position);
        }

        void MoveToPlayer()
        {
            if (_attacking||!_can_move) return;
            Vector3 dir = Player.main.position - position;
            Move(dir, _run_speed);
            Rotate(dir, _turn_speed);
            _current_pose = "run";
        }

        void TurnToPlayer()
        {
            if (_attacking || !_can_move) return;
            Vector3 dir = Player.main.position - position;
            Rotate(dir, _turn_speed);
        }

        void AttackPlayer()
        {
            if (!_attacking&&_can_move&&!_hurt)
            {
                _can_move = false;
                _attacking = true;
                _attack_combo = 0;
                _combo_timer = 0;
                _attack_timer = 0;
            }
        }

        void RefreshAttack()
        {
            if (weapon != null)
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
                        }
                        else if (_combo_timer <= 0)
                        {
                            _can_move = true;
                            _attacking = false;
                            _attack_combo = 0;
                            _combo_timer = 0;
                        }
                        if (_attack_combo >= _max_combo) _attack_combo = 0;
                    }
                    else
                    {
                        if (_attack_timer >= _attacks_time[_attack_combo] - _combo_time)
                        {
                            _attack_timer = 0;
                            _combo_timer = _combo_time;
                        }
                        Attack(_attack_combo, _attack_timer);
                        _attack_timer += Time.deltaTime;
                    }

                }
            }
        }

        bool _hurt = false;
        int _hurt_side = 0;
        float[] _hurt_time = new float[] { 1, 1, 1 };
        float _hurt_timer = 0;
        string[] _hurt_pose = new string[] { "hit_left", "hit_right", "hit_back" };
        float _stun_wait_time = 3;
        float _stun_timer = 0;

        void Hurt()
        {
            if (!_hurt) return;
            _attacking = false;
            _attack_timer = 0;
            _hurt_timer += Time.deltaTime;
            if (_hurt_timer < _hurt_time[_hurt_side])
            {
                _current_pose = _hurt_pose[_hurt_side];
            } else
            {
                _can_move = true;
                _hurt = false;
                _hurt_timer = 0;
            }
        }

        public override void OnHit(int side)
        {
            if (!_hurt)
            {
                character.PlaySound("hit_0");
                _particles.Emit(5);
                _hurt = true;
                _hurt_side = side;
                _can_move = false;
            }
        }

        float _death_time = 4.2f;
        float _death_particles_time = 3f;
        bool _death_spawned_particles=false;
        float _death_timer = 0;
        void OnDeath()
        {
            _death_timer += Time.deltaTime;
            if (_death_timer >= _death_time)
            {
                entities.Remove(this);
                Destroy(gameObject);
            } else if (_death_timer >= _death_particles_time && !_death_spawned_particles)
            {
                var s = _particles.shape;
                s.radius = 0.5f;
                _particles.Emit(25);
                _death_spawned_particles = true;
            }
            else
            {
                _current_pose = "death_0";
            }
        }

        public override void Damage(float amount)
        {
            if (!_hurt)
            {
                base.Damage(amount);
            }
        }



        //######################################################################################################

        // Character

        //######################################################################################################

        public SkinnedMeshRenderer _skinned_mesh;

        void UpdateCharacter()
        {

            character.Pose(_current_pose);
            _current_pose = "idle";
        }

    }
}