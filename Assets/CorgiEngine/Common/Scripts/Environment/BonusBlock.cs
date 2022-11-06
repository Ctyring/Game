﻿using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{	
	/// <summary>
	/// Add this class to a block and it'll behave like these Super Mario blocks that spawn something when hit from below
	/// </summary>
	[AddComponentMenu("Corgi Engine/Environment/Bonus Block")]
	public class BonusBlock : MonoBehaviour, Respawnable 
	{
		/// the object this bonus block should spawn
		[Tooltip("the object this bonus block should spawn")]
		public GameObject SpawnedObject;
		/// the number of hits the block can take
		[Tooltip("the number of hits the block can take")]
		public int NumberOfAllowedHits=3;
		/// should this object get reset when the main character dies?
		[Tooltip("should this object get reset when the main character dies?")]
		public bool ResetOnDeath = false;
		/// the speed at which the block spawns its content
		[Tooltip("the speed at which the block spawns its content")]
		public float SpawnSpeed = 0.2f;
		/// the offset position for the block's content spawn
		[Tooltip("the offset position for the block's content spawn")]
		public Vector3 SpawnDestination;
		/// if true, should animate the object's spawn
		[Tooltip("if true, should animate the object's spawn")]
		public bool AnimateSpawn = true;
		/// the block's movement Animation Curve
		[Tooltip("the block's movement Animation Curve")]
		public AnimationCurve MovementCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1f, 1));

		// private stuff
		protected Animator _animator;
		protected bool _hit=false;
		protected Vector2 _newPosition;
		protected int _numberOfHitsLeft;
		protected BoxCollider2D _boxCollider2D;
		
		/// <summary>
		/// Initialization
		/// </summary>
		public virtual void Start()
		{
			Initialization();
		}

		/// <summary>
		/// Initialization
		/// </summary>
		protected virtual void Initialization()
		{
			// we get the animator
			_animator = this.gameObject.GetComponent<Animator>();
			_boxCollider2D = this.gameObject.GetComponent<BoxCollider2D>();

			_numberOfHitsLeft =NumberOfAllowedHits;
			if (_numberOfHitsLeft>0)	
			{
				_animator.SetBool("Off", false);
			}
			else			
			{
				_animator.SetBool("Off", true);
			}
		}
		
		/// <summary>
		/// This is called every frame.
		/// </summary>
		protected virtual void Update()
		{		
			// we send our various states to the animator.		
			UpdateAnimator ();	
			_hit=false;
			
		}

		/// <summary>
		/// Updates the animator.
		/// </summary>
		protected virtual void UpdateAnimator()
		{				
			_animator.SetBool("Hit", _hit);	
		}
		
		/// <summary>
		/// Triggered when a CorgiController touches the platform
		/// </summary>
		/// <param name="controller">The corgi controller that collides with the platform.</param>		
		public virtual void OnTriggerEnter2D(Collider2D collider)
		{
			CorgiController controller = collider.GetComponent<CorgiController>();
			if (controller == null)
			{
				return;
			}				
			
			// if the block has spent all its hits, we do nothing
			if (_numberOfHitsLeft == 0)
			{
				return;
			}				
			
			if (collider.transform.position.y<transform.position.y)
			{
				// if the collider's y position is less than the block's y position, we're hitting it from below, we trigger the event
				_hit=true;
				_numberOfHitsLeft--;
				
				GameObject spawned = (GameObject)Instantiate(SpawnedObject);
				spawned.transform.position=transform.position;
				spawned.transform.rotation=Quaternion.identity;
				if (AnimateSpawn)
				{
					StartCoroutine(MMMovement.MoveFromTo(spawned,transform.position, new Vector2(transform.position.x+ SpawnDestination.x, transform.position.y + _boxCollider2D.size.y+SpawnDestination.y),SpawnSpeed, MovementCurve));
				}
				else
				{
					spawned.transform.position = transform.position + SpawnDestination;
				}						
			}
			
			if (_numberOfHitsLeft == 0)
			{			
				_animator.SetBool("Off", true);
			}
		}		
		
		/// <summary>
		/// Triggered when a CorgiController exits the platform
		/// </summary>
		/// <param name="controller">The corgi controller that collides with the platform.</param>		
		public virtual void OnTriggerExit2D(Collider2D collider)
		{
			CorgiController controller = collider.GetComponent<CorgiController>();
			if (controller == null)
			{
				return;
			}				
		}

		/// <summary>
		/// Triggered when the player respawns, resets the block if needed
		/// </summary>
		/// <param name="checkpoint">Checkpoint.</param>
		/// <param name="player">Player.</param>
		public virtual void OnPlayerRespawn(CheckPoint checkpoint, Character player)
		{
			if (ResetOnDeath)
			{
				Initialization();
			}
		}
	}
}