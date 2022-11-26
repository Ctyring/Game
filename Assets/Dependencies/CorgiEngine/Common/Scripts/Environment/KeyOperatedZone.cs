﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using UnityEngine.Events;

namespace MoreMountains.CorgiEngine
{	
	/// <summary>
	/// Add this component to a collider 2D and you'll be able to have it perform an action when 
	/// a character equipped with the specified key enters it.
	/// </summary>
	[AddComponentMenu("Corgi Engine/Environment/Key Operated Zone")]
	public class KeyOperatedZone : ButtonActivated
	{
		[MMInspectorGroup("Key", true, 22)]

		/// whether this zone actually requires a key
		[Tooltip("whether this zone actually requires a key")]
		public bool RequiresKey = true;
		/// the key ID, that will be checked against the existence (or not) of a key of the same name in the player's inventory
		[Tooltip("the key ID, that will be checked against the existence (or not) of a key of the same name in the player's inventory")]
		public string KeyID;
		/// the method that should be triggered when the key is used
		[Tooltip("the method that should be triggered when the key is used")]
		public UnityEvent KeyAction;

		protected Collider2D _collidingObject;
		protected List<int> _keyList;

		/// <summary>
		/// On Start we initialize our object
		/// </summary>
		protected virtual void Start()
		{
			_keyList = new List<int> ();
		}

		/// <summary>
		/// On enter we store our colliding object
		/// </summary>
		/// <param name="collider">Something colliding with the water.</param>
		protected override void OnTriggerEnter2D(Collider2D collider)
		{
			_collidingObject = collider;
			base.OnTriggerEnter2D (collider);
		}	

		/// <summary>
		/// When the button is pressed, we check if we have a key in our inventory
		/// </summary>
		public override void TriggerButtonAction(GameObject instigator)
		{
			if (!CheckNumberOfUses())
			{
				PromptError();
				return;
			}

			if (_collidingObject == null) { return; }

			if (RequiresKey)
			{
				CharacterInventory characterInventory = _collidingObject.gameObject.MMGetComponentNoAlloc<Character>()?.FindAbility<CharacterInventory>();
				if (characterInventory == null)
				{
					PromptError();
					return;
				}	

				_keyList.Clear ();
				_keyList = characterInventory.MainInventory.InventoryContains (KeyID);
				if (_keyList.Count == 0)
				{
					PromptError();
					return;
				}
				else
				{
					base.TriggerButtonAction (instigator);
					characterInventory.MainInventory.UseItem(KeyID);
				}
			}
			TriggerKeyAction ();
			ActivateZone ();
		}

		/// <summary>
		/// Calls the method associated to the key action
		/// </summary>
		protected virtual void TriggerKeyAction()
		{
			if (KeyAction != null)
			{
				KeyAction.Invoke ();
			}
		}
	}
}