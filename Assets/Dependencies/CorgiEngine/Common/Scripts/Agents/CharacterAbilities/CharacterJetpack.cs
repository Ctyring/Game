﻿using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{	
	/// <summary>
	/// Add this component to a character and it'll be able to jetpack
	/// Animator parameters : Jetpacking (bool)
	/// </summary>
	[AddComponentMenu("Corgi Engine/Character/Abilities/Character Jetpack")] 
	public class CharacterJetpack : CharacterAbility 
	{
		/// This method is only used to display a helpbox text at the beginning of the ability's inspector
		public override string HelpBoxText() { return "Add this component to a character and it'll be able to activate a jetpack and fly through the level. Here you can define the force to apply when jetpacking, the particle system to use, various fuel info, and optionnally what sound to play when the jetpack gets fully refueled"; }

		[Header("Jetpack")]

		/// the duration (in seconds) during which we'll disable the collider when taking off jetpacking from a moving platform
		[Tooltip("the duration (in seconds) during which we'll disable the collider when taking off jetpacking from a moving platform")]
		public float MovingPlatformsJumpCollisionOffDuration=0.05f;
		/// the jetpack associated to the character
		[Tooltip("the jetpack associated to the character")]
		public ParticleSystem ParticleEmitter;
		/// the force applied by the jetpack
		[Tooltip("the force applied by the jetpack")]
		public float JetpackForce = 2.5f;	

		[Header("Fuel")]

		/// true if the character has unlimited fuel for its jetpack
		[Tooltip("true if the character has unlimited fuel for its jetpack")]
		public bool JetpackUnlimited = false;
		/// the maximum duration (in seconds) of the jetpack
		[Tooltip("the maximum duration (in seconds) of the jetpack")]
		public float JetpackFuelDuration = 5f;
		/// the jetpack refuel cooldown, in seconds
		[Tooltip("the jetpack refuel cooldown, in seconds")]
		public float JetpackRefuelCooldown=1f;
		/// the speed at which the jetpack refuels
		[Tooltip("the speed at which the jetpack refuels")]
		public float RefuelSpeed = 0.5f;
		/// the minimum amount of fuel required in the tank to be able to jetpack again
		[Tooltip("the minimum amount of fuel required in the tank to be able to jetpack again")]
		public float MinimumFuelRequirement = 0.2f;

		[Header("Jetpack Sounds")]
		/// The sound to play when the jetpack is refueled again
		[Tooltip("The sound to play when the jetpack is refueled again")]
		public AudioClip JetpackRefueledSfx;

		[Header("Debug")]
		/// the remaining jetpack fuel duration (in seconds)
		[MMReadOnly]
		public float JetpackFuelDurationLeft = 0f;
		
		/// returns true if this jetpack still has fuel left, false otherwise
		public bool FuelLeft { get { return (JetpackUnlimited || (JetpackFuelDurationLeft > 0f)) ; } }

		protected bool _refueling = false;
		protected bool _jetpacking = true;
		protected Vector3 _initialPosition;
		protected AudioSource _jetpackUsedSound;
		protected WaitForSeconds _jetpackRefuelCooldownWFS;
		protected float _timer;
		protected float _jetpackStoppedAt;

		// animation parameters
		protected const string _jetpackingAnimationParameterName = "Jetpacking";
		protected int _jetpackingAnimationParameter;

		/// <summary>
		/// On Start(), we grab our particle emitter if there's one, and setup our fuel reserves
		/// </summary>
		protected override void Initialization () 
		{
			base.Initialization();
					
			if (ParticleEmitter!=null)
			{
				_initialPosition = ParticleEmitter.transform.localPosition;
				ParticleSystem.EmissionModule emissionModule = ParticleEmitter.emission;
				emissionModule.enabled=false;
			}
			JetpackFuelDurationLeft = JetpackFuelDuration;
			_jetpackRefuelCooldownWFS = new WaitForSeconds (JetpackRefuelCooldown);

			if (GUIManager.HasInstance && _character.CharacterType == Character.CharacterTypes.Player)
			{ 
				GUIManager.Instance.SetJetpackBar(!JetpackUnlimited, _character.PlayerID);
				UpdateJetpackBar();
			}
		}

		/// <summary>
		/// Every frame, we check input to see if we're pressing or releasing the jetpack button
		/// </summary>
		protected override void HandleInput()
		{
			if (_inputManager.JetpackButton.State.CurrentState == MMInput.ButtonStates.ButtonDown || _inputManager.JetpackButton.State.CurrentState == MMInput.ButtonStates.ButtonPressed)
			{
				JetpackStart();
			}			
			
			if ((_inputManager.JetpackButton.State.CurrentState == MMInput.ButtonStates.ButtonUp) && (_jetpacking))
			{
				JetpackStop();
			}
		}
		
		/// <summary>
		/// Causes the character to start its jetpack.
		/// </summary>
		public virtual void JetpackStart()
		{
			if ((!AbilityAuthorized) // if the ability is not permitted
			    || (!FuelLeft) // or if there's no fuel left
			    || (_movement.CurrentState == CharacterStates.MovementStates.Crawling)
			    || (_movement.CurrentState == CharacterStates.MovementStates.Crouching)
			    || (_movement.CurrentState == CharacterStates.MovementStates.LedgeHanging)
			    || (_movement.CurrentState == CharacterStates.MovementStates.Dashing)
			    || (_movement.CurrentState == CharacterStates.MovementStates.Gripping) // or if we're in the gripping state
			    || (_condition.CurrentState != CharacterStates.CharacterConditions.Normal)) // or if we're not in normal conditions
			{
				return;
			}				

			// if the jetpack is not unlimited and if we don't have fuel left
			if (!FuelLeft) 
			{
				// we stop the jetpack and exit
				JetpackStop();
				return;
			}

			// we set the vertical force
			if ((!_controller.State.IsGrounded) || (JetpackForce + _controller.ForcesApplied.y >= 0))
			{
				// if the character is standing on a moving platform and not pressing the down button,
				if ((_controller.State.IsGrounded) && (_controller.State.OnAMovingPlatform))
				{					
					// we turn the boxcollider off for a few milliseconds, so the character doesn't get stuck mid air
					StartCoroutine (_controller.DisableCollisionsWithMovingPlatforms (MovingPlatformsJumpCollisionOffDuration));
					_controller.DetachFromMovingPlatform ();
				}
				_controller.SetVerticalForce (JetpackForce);
			} 

			// if this is the first time we're here, we trigger our sounds
			if ((_movement.CurrentState != CharacterStates.MovementStates.Jetpacking) && !_startFeedbackIsPlaying)
			{
				// we play the jetpack start sound 
				PlayAbilityStartFeedbacks();
				MMCharacterEvent.Trigger(_character, MMCharacterEventTypes.Jetpack, MMCharacterEvent.Moments.Start);
				_jetpacking = true;
			}

			// we set the various states
			_movement.ChangeState(CharacterStates.MovementStates.Jetpacking);

			if (ParticleEmitter!=null)
			{
				ParticleSystem.EmissionModule emissionModule = ParticleEmitter.emission;
				emissionModule.enabled=true;
			}
		}
		
		/// <summary>
		/// Causes the character to stop its jetpack.
		/// </summary>
		public virtual void JetpackStop()
		{
			if ((!AbilityAuthorized) // if the ability is not permitted
			    || (_movement.CurrentState == CharacterStates.MovementStates.Gripping) // or if we're in the gripping state
			    || (_movement.CurrentState == CharacterStates.MovementStates.LedgeHanging) // or if we're in the ledge hanging state
			    || (_condition.CurrentState != CharacterStates.CharacterConditions.Normal)) // or if we're not in normal conditions
				return;

			TurnJetpackElementsOff ();

			// we set our current state to the previous recorded one
			_movement.RestorePreviousState();
		}

		/// <summary>
		/// Stops the jetpack sounds, particles and state
		/// </summary>
		protected virtual void TurnJetpackElementsOff()
		{
			// we play our stop sound
			if (_movement.CurrentState == CharacterStates.MovementStates.Jetpacking)
			{
				StopStartFeedbacks();
				PlayAbilityStopFeedbacks();
				MMCharacterEvent.Trigger(_character, MMCharacterEventTypes.Jetpack, MMCharacterEvent.Moments.End);
			}

			// if we have a jetpack particle emitter, we turn it off
			if (ParticleEmitter!=null)
			{
				ParticleSystem.EmissionModule emissionModule = ParticleEmitter.emission;
				emissionModule.enabled=false;
			}

			// if the jetpack is not unlimited, we start refueling
			_jetpackStoppedAt = Time.time;
			_jetpacking = false;
		}

		/// <summary>
		/// Every frame, we check if our character is colliding with the ceiling. If that's the case we cap its vertical force
		/// </summary>
		public override void ProcessAbility()
		{
			base.ProcessAbility();

			BurnFuel();
			Refuel();
			
			// if we're not jetpacking anymore, we stop our jetpacking feedback
			if ((_movement.CurrentState != CharacterStates.MovementStates.Jetpacking) && _startFeedbackIsPlaying)
			{
				StopStartFeedbacks();
				PlayAbilityStopFeedbacks();
				MMCharacterEvent.Trigger(_character, MMCharacterEventTypes.Jetpack, MMCharacterEvent.Moments.End);
			}

			if (_movement.CurrentState != CharacterStates.MovementStates.Jetpacking && _jetpacking )
			{
				TurnJetpackElementsOff ();
			}
		}

		/// <summary>
		/// Consumes fuel if needed
		/// </summary>
		protected virtual void BurnFuel()
		{
			if (!JetpackUnlimited)
			{
				if ((JetpackFuelDurationLeft > 0) && (_movement.CurrentState == CharacterStates.MovementStates.Jetpacking))
				{
					JetpackFuelDurationLeft -= Time.deltaTime;
					if (JetpackFuelDurationLeft < 0)
					{
						JetpackFuelDurationLeft = 0f;
					}
					UpdateJetpackBar();
				}	
			}
		}

		/// <summary>
		/// Refuels the jetpack if needed
		/// </summary>
		protected virtual void Refuel() 
		{
			if (JetpackUnlimited)
			{
				return;
			}

			// we wait for a while before starting to refill
			if (Time.time - _jetpackStoppedAt < JetpackRefuelCooldown)
			{
				return;
			}
			
			//_refueling = false;

			// then we progressively refill the jetpack fuel
			if ((JetpackFuelDurationLeft < JetpackFuelDuration) && (_movement.CurrentState != CharacterStates.MovementStates.Jetpacking))
			{
				//_refueling = true;
				JetpackFuelDurationLeft += Time.deltaTime * RefuelSpeed;
				UpdateJetpackBar();
				
				// if we're full, we play our refueled sound 
				if (System.Math.Abs (JetpackFuelDurationLeft - JetpackFuelDuration) < JetpackFuelDuration/100)
				{
					JetpackFuelDurationLeft = JetpackFuelDuration;
					PlayJetpackRefueledSfx ();
				}
			}
		}

		/// <summary>
		/// Updates the GUI jetpack bar.
		/// </summary>
		protected virtual void UpdateJetpackBar()
		{
			if (Application.isPlaying)
			{
				if ((GUIManager.HasInstance) && (_character.CharacterType == Character.CharacterTypes.Player))
				{
					GUIManager.Instance.UpdateJetpackBar(JetpackFuelDurationLeft, 0f, JetpackFuelDuration, _character.PlayerID);
				}
			}
		}

		/// <summary>
		/// Flips the jetpack's emitter horizontally
		/// </summary>
		public override void Flip()
		{
			if (_character == null)
			{
				Initialization ();
			}

			if (ParticleEmitter != null) 
			{
				// we invert the rotation of the particle emitter
				ParticleEmitter.transform.eulerAngles = new Vector3 (ParticleEmitter.transform.eulerAngles.x, ParticleEmitter.transform.eulerAngles.y + 180, ParticleEmitter.transform.eulerAngles.z);	

				// we mirror its position around the transform's center
				if (ParticleEmitter.transform.localPosition == _initialPosition)
				{
					ParticleEmitter.transform.localPosition = Vector3.Scale (_initialPosition, _character.ModelFlipValue);	
				} 
				else 
				{
					ParticleEmitter.transform.localPosition = _initialPosition;	
				}
			}
		}

		/// <summary>
		/// Plays a sound when the jetpack is fully refueled
		/// </summary>
		protected virtual void PlayJetpackRefueledSfx()
		{
			if (JetpackRefueledSfx != null)
			{
				MMSoundManagerSoundPlayEvent.Trigger(JetpackRefueledSfx, MMSoundManager.MMSoundManagerTracks.Sfx, this.transform.position);
			}
		}	

		/// <summary>
		/// When the character dies we stop its jetpack
		/// </summary>
		public override void ResetAbility()
		{
			// if we have a jetpack particle emitter, we turn it off
			if (ParticleEmitter!=null)
			{
				ParticleSystem.EmissionModule emissionModule = ParticleEmitter.emission;
				emissionModule.enabled=false;
			}
			StopStartFeedbacks();
			JetpackFuelDurationLeft = JetpackFuelDuration;
			UpdateJetpackBar();
			if (_animator != null)
			{
				MMAnimatorExtensions.UpdateAnimatorBool(_animator, _jetpackingAnimationParameter, false, _character._animatorParameters, _character.PerformAnimatorSanityChecks);	
			}
			_movement?.ChangeState (CharacterStates.MovementStates.Idle);
		}
        
		/// <summary>
		/// Adds required animator parameters to the animator parameters list if they exist
		/// </summary>
		protected override void InitializeAnimatorParameters()
		{
			RegisterAnimatorParameter (_jetpackingAnimationParameterName, AnimatorControllerParameterType.Bool, out _jetpackingAnimationParameter);
		}

		/// <summary>
		/// At the end of each cycle, we send our character's animator the current jetpacking status
		/// </summary>
		public override void UpdateAnimator()
		{
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _jetpackingAnimationParameter, (_movement.CurrentState == CharacterStates.MovementStates.Jetpacking), _character._animatorParameters, _character.PerformAnimatorSanityChecks);
		}
	}
}