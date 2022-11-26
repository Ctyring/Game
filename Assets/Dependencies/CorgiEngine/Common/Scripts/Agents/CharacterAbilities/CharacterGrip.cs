﻿using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{	
	/// <summary>
	/// Add this component to a character and it'll be able to grip level elements that have the Grip component
	/// Animator parameters : Gripping (bool)
	/// </summary>
	[AddComponentMenu("Corgi Engine/Character/Abilities/Character Grip")] 
	public class CharacterGrip : CharacterAbility 
	{
		/// This method is only used to display a helpbox text at the beginning of the ability's inspector
		public override string HelpBoxText() { return "Add this component to a character and it'll be able to grip level elements that have the Grip component."; }
		/// The duration (in seconds) during which a character can't grip again after exiting a grip
		[Tooltip("The duration (in seconds) during which a character can't grip again after exiting a grip")]
		public float BufferDurationAfterGrip = 0.3f;
		/// whether or not grip input has to be pressed to enter grip
		[Tooltip("whether or not grip input has to be pressed to enter grip")]
		public bool InputBasedGrip = false;
		/// Returns true if the character can grip right now, false otherwise
		public bool CanGrip { get { return (Time.time - _lastGripTimestamp > BufferDurationAfterGrip); }}

		protected CharacterJump _characterJump;
		protected float _lastGripTimestamp = 0f;
		protected Grip _gripTarget;
		protected bool _attached = false;

		// animation parameters
		protected const string _grippingAnimationParameterName = "Gripping";
		protected int _grippingAnimationParameter;

		/// <summary>
		/// On Start() we grab our character jump component
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			_characterJump = _character?.FindAbility<CharacterJump>();
		}

		/// <summary>
		/// Every frame we check to see if we should be gripping
		/// </summary>
		public override void ProcessAbility()
		{
			base.ProcessAbility();
			Grip();
			Detach ();
		}
		
		protected override void HandleInput()
		{
			if (InputBasedGrip && _attached)
			{
				if ((_inputManager.GripButton.State.CurrentState != MMInput.ButtonStates.ButtonDown) && (_inputManager.GripButton.State.CurrentState != MMInput.ButtonStates.ButtonPressed))
				{
					_movement.ChangeState(CharacterStates.MovementStates.Falling);
					_controller.GravityActive(true);	
					Detach();
				}
			}
		}

		/// <summary>
		/// A public method to have the character grip the specified target
		/// </summary>
		/// <param name="gripTarget">Grip target.</param>
		public virtual void StartGripping(Grip gripTarget)
		{
			if (!CanGrip)
			{
				return;
			}

			if (InputBasedGrip)
			{
				if ((_inputManager.GripButton.State.CurrentState != MMInput.ButtonStates.ButtonDown) && (_inputManager.GripButton.State.CurrentState != MMInput.ButtonStates.ButtonPressed))
				{
					return;
				}
			}

			PlayAbilityStartFeedbacks();
			MMCharacterEvent.Trigger(_character, MMCharacterEventTypes.Grip, MMCharacterEvent.Moments.Start);
			_attached = true;
			_gripTarget = gripTarget;
			_movement.ChangeState (CharacterStates.MovementStates.Gripping);
		}

		/// <summary>
		/// Called at update, handles gripping to Grip components (ropes, etc)
		/// </summary>
		protected virtual void Grip()
		{
			// if we're gripping to something, we disable the gravity
			if (_movement.CurrentState == CharacterStates.MovementStates.Gripping)
			{	
				_controller.SetForce(Vector2.zero);
				_controller.GravityActive(false);		
				if (_characterJump != null)
				{
					_characterJump.ResetNumberOfJumps();
				}
				_controller.SetTransformPosition(_gripTarget.transform.position + _gripTarget.GripOffset);
			}
		}	

		/// <summary>
		/// Checks whether we should stop gripping or not
		/// </summary>
		protected virtual void Detach()
		{
			if ((_movement.CurrentState != CharacterStates.MovementStates.Gripping) 
			    && (_movement.PreviousState == CharacterStates.MovementStates.Gripping)
			    && _attached)
			{
				_lastGripTimestamp = Time.time;
				_attached = false;
				StopStartFeedbacks();
				PlayAbilityStopFeedbacks();
				MMCharacterEvent.Trigger(_character, MMCharacterEventTypes.Grip, MMCharacterEvent.Moments.End);
			}
		}

		/// <summary>
		/// Adds required animator parameters to the animator parameters list if they exist
		/// </summary>
		protected override void InitializeAnimatorParameters()
		{
			RegisterAnimatorParameter (_grippingAnimationParameterName, AnimatorControllerParameterType.Bool, out _grippingAnimationParameter);
		}

		/// <summary>
		/// At the end of each cycle, we send our character's animator the current gripping status
		/// </summary>
		public override void UpdateAnimator()
		{
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _grippingAnimationParameter, (_movement.CurrentState == CharacterStates.MovementStates.Gripping), _character._animatorParameters, _character.PerformAnimatorSanityChecks);
		}

		/// <summary>
		/// On reset ability, we cancel all the changes made
		/// </summary>
		public override void ResetAbility()
		{
			base.ResetAbility();
			if (_condition.CurrentState == CharacterStates.CharacterConditions.Normal)
			{
				Detach();
			}

			if (_animator != null)
			{
				MMAnimatorExtensions.UpdateAnimatorBool(_animator, _grippingAnimationParameter, false, _character._animatorParameters, _character.PerformAnimatorSanityChecks);	
			}
		}
	}
}