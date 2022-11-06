﻿using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// This component allows your character to fly by moving gravity-free on both x and y axis. Here you can define the flight speed, as well as whether or not the character is always flying (in which case you don't have to press a button to fly). Important note : slope ceilings are not supported for now.
	/// </summary>
	[AddComponentMenu("Corgi Engine/Character/Abilities/Character Fly")]
	public class CharacterFly : CharacterAbility
	{
		public override string HelpBoxText() { return "This component allows your character to fly by moving gravity-free on both x and y axis. Here you can define the flight speed, as well as whether or not the character is always flying (in which case you don't have to press a button to fly). Important note : slope ceilings are not supported for now."; }

		/// the speed at which the character should fly
		[Tooltip("the speed at which the character should fly")]
		public float FlySpeed = 6f;
		/// a multiplier you can target to increase/reduce the flight speed
		public float MovementSpeedMultiplier { get; set; }
		/// whether or not the Character is always flying, in which case it'll start immune to gravity 
		[Tooltip("whether or not the Character is always flying, in which case it'll start immune to gravity ")]
		public bool AlwaysFlying = false;
		/// whether or not the Character should stop flying on death
		[Tooltip("whether or not the Character should stop flying on death")]
		public bool StopFlyingOnDeath = true;

		protected float _horizontalMovement;
		protected float _verticalMovement;
		protected bool _flying;
        
		// animation parameters
		protected const string _flyingAnimationParameterName = "Flying";
		protected const string _flySpeedAnimationParameterName = "FlySpeed";
		protected int _flyingAnimationParameter;
		protected int _flySpeedAnimationParameter;

		/// <summary>
		/// On Start, we initialize our flight if needed
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();

			MovementSpeedMultiplier = 1f;

			if (AlwaysFlying)
			{
				StartFlight();
			}
		}

		/// <summary>
		/// Looks for hztal and vertical input, and for flight button if needed
		/// </summary>
		protected override void HandleInput()
		{
			_horizontalMovement = _horizontalInput;
			_verticalMovement = _verticalInput;

			if (!AlwaysFlying)
			{
				if (_inputManager.FlyButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)
				{
					StartFlight();
				}

				if ((_inputManager.FlyButton.State.CurrentState == MMInput.ButtonStates.ButtonUp) && (_movement.CurrentState == CharacterStates.MovementStates.Flying))
				{
					StopFlight();
				}
			}
		}

		/// <summary>
		/// Sets the horizontal move value.
		/// </summary>
		/// <param name="value">Horizontal move value, between -1 and 1 - positive : will move to the right, negative : will move left </param>
		public virtual void SetHorizontalMove(float value)
		{
			_horizontalMovement = value;
		}

		/// <summary>
		/// Sets the horizontal move value.
		/// </summary>
		/// <param name="value">Horizontal move value, between -1 and 1 - positive : will move to the right, negative : will move left </param>
		public virtual void SetVerticalMove(float value)
		{
			_verticalMovement = value;
		}

		/// <summary>
		/// Starts the flight sequence
		/// </summary>
		public virtual void StartFlight()
		{
			if ((!AbilityAuthorized) // if the ability is not permitted
			    || (_movement.CurrentState == CharacterStates.MovementStates.Dashing) // or if we're dashing
			    || (_movement.CurrentState == CharacterStates.MovementStates.Gripping) // or if we're in the gripping state
			    || (_condition.CurrentState != CharacterStates.CharacterConditions.Normal)) // or if we're not in normal conditions
			{
				return;
			}

			// if this is the first time we're here, we trigger our sounds
			if (_movement.CurrentState != CharacterStates.MovementStates.Flying)
			{
				// we play the jetpack start sound 
				PlayAbilityStartFeedbacks();
				MMCharacterEvent.Trigger(_character, MMCharacterEventTypes.Fly, MMCharacterEvent.Moments.Start);
				_flying = true;
			}

			// we set the various states
			_movement.ChangeState(CharacterStates.MovementStates.Flying);

			MovementSpeedMultiplier = 1f;
			_controller.GravityActive(false);
		}

		/// <summary>
		/// Stops the flight
		/// </summary>
		public virtual void StopFlight()
		{
			_flying = false;
			if (_movement.CurrentState == CharacterStates.MovementStates.Flying)
			{
				StopStartFeedbacks();
				PlayAbilityStopFeedbacks();
				MMCharacterEvent.Trigger(_character, MMCharacterEventTypes.Fly, MMCharacterEvent.Moments.End);
			}
			if (_movement.CurrentState == CharacterStates.MovementStates.LadderClimbing)
			{
				return;
			}
			_controller.GravityActive(true);
			_movement.RestorePreviousState();
		}

		/// <summary>
		/// On Update, checks if we should stop flying
		/// </summary>
		public override void ProcessAbility()
		{
			base.ProcessAbility();

			if (StopFlyingOnDeath && (_character.ConditionState.CurrentState == CharacterStates.CharacterConditions.Dead))
			{
				return;
			}

			if (AlwaysFlying)
			{
				if (_movement.CurrentState != CharacterStates.MovementStates.Flying)
				{
					_movement.ChangeState(CharacterStates.MovementStates.Flying);
				}                
				_flying = true;
			}

			if (_flying)
			{
				_controller.GravityActive(false);
			}

			HandleMovement();
            
			// if we're not walking anymore, we stop our walking sound
			if (_movement.CurrentState != CharacterStates.MovementStates.Flying && _startFeedbackIsPlaying)
			{
				StopStartFeedbacks();
			}

			if (_movement.CurrentState != CharacterStates.MovementStates.Flying && _flying)
			{
				StopFlight();
			}

			if (_controller.State.IsCollidingAbove && (_movement.CurrentState != CharacterStates.MovementStates.Flying))
			{
				_controller.SetVerticalForce(0);
			}
		}


		/// <summary>
		/// Makes the character move in the air
		/// </summary>
		protected virtual void HandleMovement()
		{
			// if we're not walking anymore, we stop our walking sound
			if (_movement.CurrentState != CharacterStates.MovementStates.Flying && _startFeedbackIsPlaying)
			{
				StopStartFeedbacks();
			}

			// if movement is prevented, or if the character is dead/frozen/can't move, we exit and do nothing
			if (!AbilityAuthorized
			    || (_condition.CurrentState != CharacterStates.CharacterConditions.Normal)
			    || (_movement.CurrentState == CharacterStates.MovementStates.Gripping))
			{
				return;
			}
            
			// If the value of the horizontal axis is positive, the character must face right.
			if (_horizontalMovement > 0.1f)
			{
				if (!_character.IsFacingRight)
					_character.Flip();
			}
			// If it's negative, then we're facing left
			else if (_horizontalMovement < -0.1f)
			{
				if (_character.IsFacingRight)
					_character.Flip();
			}
            
			if (_flying)
			{
				// we pass the horizontal force that needs to be applied to the controller.
				float horizontalMovementSpeed = _horizontalMovement * FlySpeed * _controller.Parameters.SpeedFactor * MovementSpeedMultiplier;
				float verticalMovementSpeed = _verticalMovement * FlySpeed * _controller.Parameters.SpeedFactor * MovementSpeedMultiplier;

				// we set our newly computed speed to the controller
				_controller.SetHorizontalForce(horizontalMovementSpeed);
				_controller.SetVerticalForce(verticalMovementSpeed);
			}            
		}

		/// <summary>
		/// When the character respawns we reinitialize it
		/// </summary>
		protected virtual void OnRevive()
		{
			Initialization();
		}

		/// <summary>
		/// On death the character stops flying if needed
		/// </summary>
		protected override void OnDeath()
		{
			base.OnDeath();
			if (StopFlyingOnDeath)
			{
				StopFlight();
			}
		}

		/// <summary>
		/// When the player respawns, we reinstate this agent.
		/// </summary>
		/// <param name="checkpoint">Checkpoint.</param>
		/// <param name="player">Player.</param>
		protected override void OnEnable()
		{
			base.OnEnable();
			if (gameObject.GetComponentInParent<Health>() != null)
			{
				gameObject.GetComponentInParent<Health>().OnRevive += OnRevive;
			}
		}

		/// <summary>
		/// Stops listening for revive events
		/// </summary>
		protected override void OnDisable()
		{
			base.OnDisable();
			if (_health != null)
			{
				_health.OnRevive -= OnRevive;
			}
		}

		/// <summary>
		/// Adds required animator parameters to the animator parameters list if they exist
		/// </summary>
		protected override void InitializeAnimatorParameters()
		{
			RegisterAnimatorParameter(_flyingAnimationParameterName, AnimatorControllerParameterType.Bool, out _flyingAnimationParameter);
			RegisterAnimatorParameter(_flySpeedAnimationParameterName, AnimatorControllerParameterType.Float, out _flySpeedAnimationParameter);
		}

		/// <summary>
		/// At the end of each cycle, we send our character's animator the current flying status
		/// </summary>
		public override void UpdateAnimator()
		{
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _flyingAnimationParameter, (_movement.CurrentState == CharacterStates.MovementStates.Flying), _character._animatorParameters, _character.PerformAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _flySpeedAnimationParameter, Mathf.Abs(_controller.Speed.magnitude), _character._animatorParameters, _character.PerformAnimatorSanityChecks);
		}

		/// <summary>
		/// On reset ability, we cancel all the changes made
		/// </summary>
		public override void ResetAbility()
		{
			base.ResetAbility();
			StopFlight();
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _flyingAnimationParameter, false, _character._animatorParameters, _character.PerformAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _flySpeedAnimationParameter, 0f, _character._animatorParameters, _character.PerformAnimatorSanityChecks);
		}
	}
}