﻿using System;
using UnityEngine;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;
using System.Collections.Generic;
using UnityEngine.Events;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// Extend this class to activate something when a button is pressed in a certain zone
	/// </summary>
	[MMRequiresConstantRepaint]
	[RequireComponent(typeof(Collider2D))]
	public class ButtonActivated : MMMonoBehaviour
	{
		/// the different possible requirements for this activated zone, which can be either a character, a button activator, one or the other, or none
		public enum ButtonActivatedRequirements { Character, ButtonActivator, Either, None }
		/// how input gets detected for this zone (default : default binding from the InputManager for Interact, button (type in an axis button name), or key)
		public enum InputTypes { Default, Button, Key }
        
		[MMInspectorGroup("Requirements", true, 10)]
		/// the requirement(s) for this zone
		[Tooltip("the requirement(s) for this zone")]
		public ButtonActivatedRequirements ButtonActivatedRequirement = ButtonActivatedRequirements.Either;
		/// if this is true, this can only be activated by player Characters
		[Tooltip("if this is true, this can only be activated by player Characters")]
		public bool RequiresPlayerType = true;
		/// if this is true, this zone can only be activated if the character has the required ability
		[Tooltip("if this is true, this zone can only be activated if the character has the required ability")]
		public bool RequiresButtonActivationAbility = true;
		/// if this is true, characters won't be able to jump when in that zone, regardless of the settings on their CharacterButtonActivation ability
		[Tooltip("if this is true, characters won't be able to jump when in that zone, regardless of the settings on their CharacterButtonActivation ability")]
		public bool PreventJumpsWhileInThisZone = false;

		[MMInspectorGroup("Activation Conditions", true, 11)]
		[MMInformation("Here you can specific how that zone is interacted with. Does it require the ButtonActivation character ability? Can it only be interacted with by the Player? Does it require a button press? Can it only be activated while standing on the ground?", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]

		/// if this is false, the zone won't be activable 
		[Tooltip("if this is false, the zone won't be activable ")]
		public bool Activable = true;
		/// if true, the zone will activate whether the button is pressed or not
		[Tooltip("if true, the zone will activate whether the button is pressed or not")]
		public bool AutoActivation = false;
		/// if true, this zone will be auto activated but will still allow button interaction
		[Tooltip("if true, this zone will be auto activated but will still allow button interaction")]
		[MMCondition("AutoActivation", true)]
		public bool AutoActivationAndButtonInteraction = false;
		/// if this is set to false, the zone won't be activable while not grounded
		[Tooltip("if this is set to false, the zone won't be activable while not grounded")]
		public bool CanOnlyActivateIfGrounded = false;
		/// Set this to true if you want the CharacterBehaviorState to be notified of the player's entry into the zone.
		[Tooltip("Set this to true if you want the CharacterBehaviorState to be notified of the player's entry into the zone.")]
		public bool ShouldUpdateState = true;
		/// if this is true, enter won't be retriggered if another object enters, and exit will only be triggered when the last object exits
		[Tooltip("if this is true, enter won't be retriggered if another object enters, and exit will only be triggered when the last object exits")]
		public bool OnlyOneActivationAtOnce = true;
		/// if this is true, extra enter checks will be performed on TriggerStay, to handle edge cases like a zone that'd prevent activation when not grounded, and a character enters it airborne, but then lands
		[Tooltip("if this is true, extra enter checks will be performed on TriggerStay, to handle edge cases like a zone that'd prevent activation when not grounded, and a character enters it airborne, but then lands")]
		public bool AlsoPerformChecksOnStay = false;

		[MMInspectorGroup("Number of Activations", true, 12)]
		[MMInformation("You can decide to have that zone be interactable forever, or just a limited number of times, and can specify a delay between uses (in seconds).", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]

		/// if this is set to false, your number of activations will be MaxNumberOfActivations
		[Tooltip("if this is set to false, your number of activations will be MaxNumberOfActivations")]
		public bool UnlimitedActivations = true;
		/// the number of times the zone can be interacted with
		[Tooltip("the number of times the zone can be interacted with")]
		public int MaxNumberOfActivations = 0;
		/// the delay (in seconds) after an activation during which the zone can't be activated
		[Tooltip("the delay (in seconds) after an activation during which the zone can't be activated")]
		public float DelayBetweenUses = 0f;
		/// if this is true, the zone will disable itself (forever or until you manually reactivate it) after its last use
		[Tooltip("if this is true, the zone will disable itself (forever or until you manually reactivate it) after its last use")]
		public bool DisableAfterUse = false;

		[MMInspectorGroup("Input", true, 13)]

		/// the selected input type (default : default binding from the InputManager for Interact, button (type in an axis button name), or key)
		[Tooltip("the selected input type (default : default binding from the InputManager for Interact, button (type in an axis button name), or key)")]
		public InputTypes InputType = InputTypes.Default;
		
		
		#if ENABLE_INPUT_SYSTEM
			/// the input action to use for this button activated object
			public InputAction InputSystemAction;
		#else
			/// the button axis name to use for this button activated object
			[MMEnumCondition("InputType", (int)InputTypes.Button)]
			[Tooltip("the button axis name to use for this button activated object")]
			public string InputButton = "Interact";
			/// the key to use for this 
			[MMEnumCondition("InputType", (int)InputTypes.Key)]
			[Tooltip("the key to use for this ")]
			public KeyCode InputKey = KeyCode.Space;
		#endif

		[MMInspectorGroup("Animation", true, 14)]

		/// an (absolutely optional) animation parameter that can be triggered on the character when activating the zone
		[Tooltip("an (absolutely optional) animation parameter that can be triggered on the character when activating the zone")]
		public string AnimationTriggerParameterName;

		[MMInspectorGroup("Visual Prompt", true, 15)]
		[MMInformation("You can have this zone show a visual prompt to indicate to the player that it's interactable.", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]

		/// if this is true, a prompt will be shown if setup properly
		[Tooltip("if this is true, a prompt will be shown if setup properly")]
		public bool UseVisualPrompt = true;
		/// the gameobject to instantiate to present the prompt
		[MMCondition("UseVisualPrompt", true)]
		[Tooltip("the gameobject to instantiate to present the prompt")]
		public ButtonPrompt ButtonPromptPrefab;
		/// the text to display in the button prompt
		[MMCondition("UseVisualPrompt", true)]
		[Tooltip("the text to display in the button prompt")]
		public string ButtonPromptText = "A";
		/// the text to display in the button prompt
		[MMCondition("UseVisualPrompt", true)]
		[Tooltip("the text to display in the button prompt")]
		public Color ButtonPromptColor = MMColors.LawnGreen;
		/// the color for the prompt's text
		[MMCondition("UseVisualPrompt", true)]
		[Tooltip("the color for the prompt's text")]
		public Color ButtonPromptTextColor = MMColors.White;
		/// If true, the "buttonA" prompt will always be shown, whether the player is in the zone or not.
		[MMCondition("UseVisualPrompt", true)]
		[Tooltip("If true, the 'buttonA' prompt will always be shown, whether the player is in the zone or not.")]
		public bool AlwaysShowPrompt = true;
		/// If true, the "buttonA" prompt will be shown when a player is colliding with the zone
		[MMCondition("UseVisualPrompt", true)]
		[Tooltip("If true, the 'buttonA' prompt will be shown when a player is colliding with the zone")]
		public bool ShowPromptWhenColliding = true;
		/// If true, the prompt will hide after use
		[MMCondition("UseVisualPrompt", true)]
		[Tooltip("If true, the prompt will hide after use")]
		public bool HidePromptAfterUse = false;
		/// the position of the actual buttonA prompt relative to the object's center
		[MMCondition("UseVisualPrompt", true)]
		[Tooltip("the position of the actual buttonA prompt relative to the object's center")]
		public Vector3 PromptRelativePosition = Vector3.zero;

		[MMInspectorGroup("Feedbacks", true, 16)]

		/// a feedback to play when the zone gets activated
		[Tooltip("a feedback to play when the zone gets activated")]
		public MMFeedbacks ActivationFeedback;
		/// a feedback to play when the zone tries to get activated but can't
		[Tooltip("a feedback to play when the zone tries to get activated but can't")]
		public MMFeedbacks DeniedFeedback;
		/// a feedback to play when the zone gets entered
		[Tooltip("a feedback to play when the zone gets entered")]
		public MMFeedbacks EnterFeedback;
		/// a feedback to play when the zone gets exited
		[Tooltip("a feedback to play when the zone gets exited")]
		public MMFeedbacks ExitFeedback;

		[MMInspectorGroup("Actions", true, 17)]

		/// an action to trigger when this gets activated
		[Tooltip("an action to trigger when this gets activated")]
		public UnityEvent OnActivation;
		/// an action to trigger when exiting this zone
		[Tooltip("an action to trigger when exiting this zone")]
		public UnityEvent OnExit;
		/// an action to trigger when staying in the zone
		[Tooltip("an action to trigger when staying in the zone")]
		public UnityEvent OnStay;

		protected Animator _buttonPromptAnimator;
		protected ButtonPrompt _buttonPrompt;
		protected bool _promptHiddenForever = false;
		protected int _numberOfActivationsLeft;
		protected float _lastActivationTimestamp;
		protected Collider2D _buttonActivatedZoneCollider;

		protected CharacterButtonActivation _characterButtonActivation;
		protected Character _currentCharacter;

		protected List<GameObject> _collidingObjects;
		protected List<GameObject> _stayingGameObjects;
		protected List<Collider2D> _enteredColliders;
		
		public bool InputActionPerformed
		{
			get
			{
				#if ENABLE_INPUT_SYSTEM
					return InputSystemAction.WasPerformedThisFrame();
				#else
					return false;
				#endif
			}
		}
		protected int _inputActionPressedAtFrame;

		/// <summary>
		/// On Enable, we initialize our ButtonActivated zone
		/// </summary>
		protected virtual void OnEnable()
		{
			Initialization();
		}

		/// <summary>
		/// Grabs components and shows prompt if needed
		/// </summary>
		public virtual void Initialization()
		{
			_collidingObjects = new List<GameObject>();
			_enteredColliders = new List<Collider2D>();
			_stayingGameObjects = new List<GameObject>();
			_buttonActivatedZoneCollider = this.gameObject.GetComponent<Collider2D>();
			_numberOfActivationsLeft = MaxNumberOfActivations;

			if (AlwaysShowPrompt)
			{
				ShowPrompt();
			}
			ActivationFeedback?.Initialization(this.gameObject);
			DeniedFeedback?.Initialization(this.gameObject);
			EnterFeedback?.Initialization(this.gameObject);
			ExitFeedback?.Initialization(this.gameObject);
			
			#if ENABLE_INPUT_SYSTEM
				InputSystemAction.Enable();
			#endif
		}

		/// <summary>
		/// On disable we disable our input action if needed
		/// </summary>
		protected virtual void OnDisable()
		{
			#if ENABLE_INPUT_SYSTEM
				InputSystemAction.Disable();
			#endif
		}

		/// <summary>
		/// Makes the zone activable
		/// </summary>
		public virtual void MakeActivable()
		{
			Activable = true;
		}

		/// <summary>
		/// Makes the zone unactivable
		/// </summary>
		public virtual void MakeUnactivable()
		{
			Activable = false;
		}

		/// <summary>
		/// Makes the zone activable if it wasn't, unactivable if it was activable.
		/// </summary>
		public virtual void ToggleActivable()
		{
			Activable = !Activable;
		}

		/// <summary>
		/// When the input button is pressed, we check whether or not the zone can be activated, and if yes, trigger ZoneActivated
		/// </summary>
		public virtual void TriggerButtonAction(GameObject instigator)
		{
			if (!CheckNumberOfUses())
			{
				PromptError();
				return;
			}

			_stayingGameObjects.Add(instigator);
			ActivateZone();
		}

		/// <summary>
		/// On exit, we reset our staying bool and invoke our OnExit event
		/// </summary>
		/// <param name="collider"></param>
		public virtual void TriggerExitAction(GameObject collider)
		{
			_stayingGameObjects.Remove(collider);
			if (OnExit != null)
			{
				OnExit.Invoke();
			}
		}

		/// <summary>
		/// Activates the zone
		/// </summary>
		protected virtual void ActivateZone()
		{
			if (OnActivation != null)
			{
				OnActivation.Invoke();
			}
			_lastActivationTimestamp = Time.time;
			if (HidePromptAfterUse)
			{
				_promptHiddenForever = true;
				HidePrompt();
			}

			ActivationFeedback?.PlayFeedbacks(this.transform.position);
			_numberOfActivationsLeft--;

			DisableAfterActivation();
		}

		/// <summary>
		/// Handles the disabling of the zone after activation
		/// </summary>
		protected virtual void DisableAfterActivation()
		{
			if (DisableAfterUse && (_numberOfActivationsLeft <= 0))
			{
				DisableZone();
			}
		}

		/// <summary>
		/// Triggers an error 
		/// </summary>
		public virtual void PromptError()
		{
			if (_buttonPromptAnimator != null)
			{
				_buttonPromptAnimator.SetTrigger("Error");
			}
			DeniedFeedback?.PlayFeedbacks(this.transform.position);
		}

		/// <summary>
		/// Shows the button A prompt.
		/// </summary>
		public virtual void ShowPrompt()
		{            
			if (!UseVisualPrompt || _promptHiddenForever || (ButtonPromptPrefab == null))
			{
				return;
			}

			// we add a blinking A prompt to the top of the zone
			if (_buttonPrompt == null)
			{
				_buttonPrompt = (ButtonPrompt)Instantiate(ButtonPromptPrefab);
				_buttonPrompt.Initialization();
				_buttonPromptAnimator = _buttonPrompt.gameObject.MMGetComponentNoAlloc<Animator>();
			}

			if (_buttonActivatedZoneCollider != null)
			{
				_buttonPrompt.transform.position = _buttonActivatedZoneCollider.bounds.center + PromptRelativePosition;
			}
			_buttonPrompt.transform.parent = transform;
			_buttonPrompt.SetText(ButtonPromptText);
			_buttonPrompt.SetBackgroundColor(ButtonPromptColor);
			_buttonPrompt.SetTextColor(ButtonPromptTextColor);
			_buttonPrompt.Show();
		}

		/// <summary>
		/// Hides the button A prompt.
		/// </summary>
		public virtual void HidePrompt()
		{
			if ((_buttonPrompt != null) && (_buttonPrompt.isActiveAndEnabled))
			{
				_buttonPrompt.Hide();
			}            
		}

		/// <summary>
		/// Enables the button activated zone
		/// </summary>
		public virtual void DisableZone()
		{
			Activable = false;
			_buttonActivatedZoneCollider.enabled = false;
			if (ShouldUpdateState && (_characterButtonActivation != null))
			{
				_characterButtonActivation.InButtonActivatedZone = false;
				_characterButtonActivation.ButtonActivatedZone = null;
			}
		}

		/// <summary>
		/// Disables the button activated zone
		/// </summary>
		public virtual void EnableZone()
		{
			Activable = true;
			_buttonActivatedZoneCollider.enabled = true;
		}

		/// <summary>
		/// Handles enter collision with 2D triggers
		/// </summary>
		/// <param name="collidingObject">Colliding object.</param>
		protected virtual void OnTriggerEnter2D(Collider2D collidingObject)
		{
			_enteredColliders.Add(collidingObject);
			TriggerEnter(collidingObject.gameObject);
		}

		/// <summary>
		/// On stay we invoke our stay event if needed, and perform a trigger enter check if it hasn't been done already
		/// </summary>
		/// <param name="collidingObject"></param>
		protected virtual void OnTriggerStay2D(Collider2D collidingObject)
		{
			if (!_enteredColliders.Contains(collidingObject))
			{
				return;
			}

			bool staying = _stayingGameObjects.Contains(collidingObject.gameObject);

			if (staying && (OnStay != null))
			{
				OnStay.Invoke();
			}
        
			if (staying || !AlsoPerformChecksOnStay)
			{
				return;
			}
			TriggerEnter(collidingObject.gameObject);
		}

		/// <summary>
		/// Handles enter collision with 2D triggers
		/// </summary>
		/// <param name="collidingObject">Colliding object.</param>
		protected virtual void OnTriggerExit2D(Collider2D collidingObject)
		{
			_enteredColliders.Remove(collidingObject);
			TriggerExit(collidingObject.gameObject);
		}

		/// <summary>
		/// Triggered when something collides with the button activated zone
		/// </summary>
		/// <param name="collider">Something colliding with the water.</param>
		protected virtual void TriggerEnter(GameObject collider)
		{    
			if (!CheckConditions(collider))
			{
				return;
			}
           
			// at this point the object is colliding and authorized, we add it to our list
			_collidingObjects.Add(collider.gameObject);
			if (!TestForLastObject(collider))
			{
				return;
			}

			_currentCharacter = collider.gameObject.MMGetComponentNoAlloc<Character>();

			if (ShouldUpdateState)
			{
				_characterButtonActivation = _currentCharacter?.FindAbility<CharacterButtonActivation>();
				if (_characterButtonActivation != null)
				{
					_characterButtonActivation.InButtonActivatedZone = true;
					_characterButtonActivation.ButtonActivatedZone = this;
					_characterButtonActivation.InButtonAutoActivatedZone = AutoActivation;
					_characterButtonActivation.InJumpPreventingZone = PreventJumpsWhileInThisZone;
					_characterButtonActivation.SetTriggerParameter();
				}
			}

			EnterFeedback?.PlayFeedbacks(this.transform.position);

			if (AutoActivation)
			{
				TriggerButtonAction(collider);
			}

			// if we're not already showing the prompt and if the zone can be activated, we show it
			if (ShowPromptWhenColliding)
			{
				ShowPrompt();
			}
		}
        
		/// <summary>
		/// Triggered when something exits the water
		/// </summary>
		/// <param name="collider">Something colliding with the dialogue zone.</param>
		protected virtual void TriggerExit(GameObject collider)
		{
			// we check that the object colliding with the water is actually a CorgiController and a character
            
			if (!CheckConditions(collider))
			{
				return;
			}

			_collidingObjects.Remove(collider.gameObject);
			if (!TestForLastObject(collider))
			{
				return;
			}

			_currentCharacter = null;

			if (ShouldUpdateState)
			{
				_characterButtonActivation = collider.gameObject.MMGetComponentNoAlloc<Character>()?.FindAbility<CharacterButtonActivation>();
				if (_characterButtonActivation != null)
				{
					_characterButtonActivation.InButtonActivatedZone = false;
					_characterButtonActivation.ButtonActivatedZone = null;
					_characterButtonActivation.InButtonAutoActivatedZone = false;
					_characterButtonActivation.InJumpPreventingZone = false;
				}
			}

			ExitFeedback?.PlayFeedbacks(this.transform.position);

			if ((_buttonPrompt != null) && !AlwaysShowPrompt)
			{
				HidePrompt();
			}

			TriggerExitAction(collider);
		}

		/// <summary>
		/// Tests if the object exiting our zone is the last remaining one
		/// </summary>
		/// <param name="collider"></param>
		/// <returns></returns>
		protected virtual bool TestForLastObject(GameObject collider)
		{
			if (OnlyOneActivationAtOnce)
			{
				if (_collidingObjects.Count > 0)
				{
					bool lastObject = true;
					foreach (GameObject obj in _collidingObjects)
					{
						if ((obj != null) && (obj != collider))
						{
							lastObject = false;
						}
					}
					return lastObject;
				}
			}
			return true;
		}

		/// <summary>
		/// Checks the remaining number of uses and eventual delay between uses and returns true if the zone can be activated.
		/// </summary>
		/// <returns><c>true</c>, if number of uses was checked, <c>false</c> otherwise.</returns>
		public virtual bool CheckNumberOfUses()
		{
			if (!Activable)
			{
				return false;
			}
            
			if ( (_currentCharacter != null) 
			     && (CanOnlyActivateIfGrounded) 
			     && (!_currentCharacter.gameObject.MMGetComponentNoAlloc<CorgiController>().State.IsGrounded) )
			{
				return false;
			}

			if (Time.time - _lastActivationTimestamp < DelayBetweenUses)
			{
				return false;
			}

			if (UnlimitedActivations)
			{
				return true;
			}

			if (_numberOfActivationsLeft == 0)
			{
				return false;
			}

			if (_numberOfActivationsLeft > 0)
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// Determines whether or not this zone should be activated
		/// </summary>
		/// <returns><c>true</c>, if conditions was checked, <c>false</c> otherwise.</returns>
		/// <param name="character">Character.</param>
		/// <param name="characterButtonActivation">Character button activation.</param>
		protected virtual bool CheckConditions(GameObject collider)
		{
			Character character = collider.gameObject.MMGetComponentNoAlloc<Character>();

			switch (ButtonActivatedRequirement)
			{
				case ButtonActivatedRequirements.Character:
					if (character == null)
					{
						return false;
					}
					break;

				case ButtonActivatedRequirements.ButtonActivator:
					if (collider.gameObject.MMGetComponentNoAlloc<ButtonActivator>() == null)
					{
						return false;
					}
					break;

				case ButtonActivatedRequirements.Either:
					if ((character == null) && (collider.gameObject.MMGetComponentNoAlloc<ButtonActivator>() == null))
					{
						return false;
					}
					break;
			}

			if (RequiresPlayerType)
			{
				if (character == null)
				{
					return false;
				}
				if (character.CharacterType != Character.CharacterTypes.Player)
				{
					return false;
				}
			}

			if (RequiresButtonActivationAbility)
			{
				CharacterButtonActivation characterButtonActivation = collider.gameObject.MMGetComponentNoAlloc<Character>()?.FindAbility<CharacterButtonActivation>();
				// we check that the object colliding with the water is actually a CorgiController and a character
				if (characterButtonActivation == null)
				{
					return false;
				}
			}

			return true;
		}
	}
}