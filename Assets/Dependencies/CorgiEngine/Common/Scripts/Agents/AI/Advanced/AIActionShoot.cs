﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// An Action that shoots using the currently equipped weapon. If your weapon is in auto mode, will shoot until you exit the state, and will only shoot once in SemiAuto mode. You can optionnally have the character face (left/right) the target, and aim at it (if the weapon has a WeaponAim component).
	/// </summary>
	[AddComponentMenu("Corgi Engine/Character/AI/Actions/AI Action Shoot")]
	// [RequireComponent(typeof(Character))]
	// [RequireComponent(typeof(CharacterHandleWeapon))]
	public class AIActionShoot : AIAction
	{
		/// if true, the Character will face the target (left/right) when shooting
		[Tooltip("if true, the Character will face the target (left/right) when shooting")]
		public bool FaceTarget = true;
		/// if true the Character will aim at the target when shooting
		[Tooltip("if true the Character will aim at the target when shooting")]
		public bool AimAtTarget = false;
		/// a constant offset to apply to the target's position when aiming : 0,1,0 would aim more towards the head, for example
		[Tooltip("a constant offset to apply to the target's position when aiming : 0,1,0 would aim more towards the head, for example")]
		public Vector3 TargetOffset = Vector3.zero;
		/// the ability to pilot
		[Tooltip("the ability to pilot")]
		public CharacterHandleWeapon TargetHandleWeapon;

		protected Character _character;
		protected WeaponAim _weaponAim;
		protected ProjectileWeapon _projectileWeapon;
		protected Vector3 _weaponAimDirection;
		protected int _numberOfShoots = 0;
		protected bool _shooting = false;

		/// <summary>
		/// On init we grab our CharacterHandleWeapon ability
		/// </summary>
		public override void Initialization()
		{
			_character = GetComponentInParent<Character>();
			if (TargetHandleWeapon == null)
			{
				TargetHandleWeapon = _character?.FindAbility<CharacterHandleWeapon>();    
			}
		}

		/// <summary>
		/// On PerformAction we face and aim if needed, and we shoot
		/// </summary>
		public override void PerformAction()
		{
			TestFaceTarget();
			TestAimAtTarget();
			Shoot();
		}

		/// <summary>
		/// Sets the current aim if needed
		/// </summary>
		protected virtual void Update()
		{
			if (TargetHandleWeapon == null)
			{
				return;
			}
			if (TargetHandleWeapon.CurrentWeapon != null)
			{
				if (_weaponAim != null)
				{
					if (_shooting)
					{
						_weaponAim.SetCurrentAim(_weaponAimDirection);
					}
					else
					{
						if (_character.IsFacingRight)
						{
							_weaponAim.SetCurrentAim(Vector3.right);
						}
						else
						{
							_weaponAim.SetCurrentAim(Vector3.left);
						}
					}
				}
			}
		}

		/// <summary>
		/// Faces the target if required
		/// </summary>
		protected virtual void TestFaceTarget()
		{
			if (!FaceTarget)
			{
				return;
			}

			if (this.transform.position.x > _brain.Target.position.x)
			{
				_character.Face(Character.FacingDirections.Left);
			}
			else
			{
				_character.Face(Character.FacingDirections.Right);
			}            
		}

		/// <summary>
		/// Aims at the target if required
		/// </summary>
		protected virtual void TestAimAtTarget()
		{
			if (!AimAtTarget)
			{
				return;
			}

			if (TargetHandleWeapon.CurrentWeapon != null)
			{
				if (_weaponAim == null)
				{
					_weaponAim = TargetHandleWeapon.CurrentWeapon.gameObject.MMGetComponentNoAlloc<WeaponAim>();
				}

				if (_weaponAim != null)
				{
					if (_projectileWeapon != null)
					{
						_projectileWeapon.DetermineSpawnPosition();
						_weaponAimDirection = (_brain.Target.position + TargetOffset) - (_projectileWeapon.SpawnPosition);
					}
					else
					{
						_weaponAimDirection = (_brain.Target.position + TargetOffset) - TargetHandleWeapon.CurrentWeapon.transform.position;
					}                    
				}                
			}
		}

		/// <summary>
		/// Activates the weapon
		/// </summary>
		protected virtual void Shoot()
		{
			if (_numberOfShoots < 1)
			{
				TargetHandleWeapon.ShootStart();
				_numberOfShoots++;
			}
		}

		/// <summary>
		/// When entering the state we reset our shoot counter and grab our weapon
		/// </summary>
		public override void OnEnterState()
		{
			base.OnEnterState();
			_numberOfShoots = 0;
			_shooting = true;
			_weaponAim = TargetHandleWeapon.CurrentWeapon.gameObject.MMGetComponentNoAlloc<WeaponAim>();
			_projectileWeapon = TargetHandleWeapon.CurrentWeapon.gameObject.MMGetComponentNoAlloc<ProjectileWeapon>();
		}

		/// <summary>
		/// When exiting the state we make sure we're not shooting anymore
		/// </summary>
		public override void OnExitState()
		{
			base.OnExitState();
			TargetHandleWeapon?.ForceStop();
			_shooting = false;
		}
	}
}