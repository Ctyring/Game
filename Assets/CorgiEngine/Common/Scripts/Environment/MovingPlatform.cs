﻿using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// Add this component to a platform and it'll be able to follow a path and carry a character
	/// </summary>
	[AddComponentMenu("Corgi Engine/Environment/Moving Platform")]
	public class MovingPlatform : MMPathMovement, Respawnable, MMEventListener<CorgiEngineEvent>
	{
		[Header("Activation")]
		[MMInformation("Check the <b>Only Moves When Player Is Colliding</b> checkbox to have the object wait for a collision with your player to start moving.",MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]

		/// If true, the object will only move when colliding with the player
		[Tooltip("If true, the object will only move when colliding with the player")]
		public bool OnlyMovesWhenPlayerIsColliding = false;
		/// if true, the object will only move if a character is colliding
		[Tooltip("if true, the object will only move if a character is colliding")]
		public bool OnlyMovesWhenCharacterIsColliding = false;
		/// If true, this moving platform will reset position and behaviour when the player respawns
		[Tooltip("If true, this moving platform will reset position and behaviour when the player respawns")]
		public bool ResetPositionWhenPlayerRespawns = false;
		/// If true, this platform will only moved when commanded to by another script
		[Tooltip("If true, this platform will only moved when commanded to by another script")]
		public bool ScriptActivated = false;
		/// If true, the object will start moving when a player collides with it. This requires that ScriptActivated be set to true (and it will set it to true on init otherwise)
		[Tooltip("If true, the object will start moving when a player collides with it. This requires that ScriptActivated be set to true (and it will set it to true on init otherwise)")]
		public bool StartMovingWhenPlayerIsColliding = false;

		[MMInspectorButton("ToggleMovementAuthorization")]
		public bool ToggleButton;
		[MMInspectorButton("ChangeDirection")]
		public bool ChangeDirectionButton;
		[MMInspectorButton("ResetEndReached")]
		public bool ResetEndReachedButton;
		[MMInspectorButton("MoveTowardsStart")]
		public bool MoveTowardsStartButton;
		[MMInspectorButton("MoveTowardsEnd")]
		public bool MoveTowardsEndButton;

		[Header("Feedbacks")]

		/// a feedback to trigger when a new point is reached
		[Tooltip("a feedback to trigger when a new point is reached")]
		public MMFeedbacks PointReachedFeedback;
		/// a feedback to trigger when the end of the path is reached
		[Tooltip("a feedback to trigger when the end of the path is reached")]
		public MMFeedbacks EndReachedFeedback;

		protected Collider2D _collider2D;
		protected float _platformTopY;
		protected const float _toleranceY = 0.05f;
		protected bool _scriptActivatedAuthorization = false;

		/// <summary>
		/// Flag inits, initial movement determination, and object positioning
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization ();
			PointReachedFeedback?.Initialization(this.gameObject);
			EndReachedFeedback?.Initialization(this.gameObject);
			_collider2D = GetComponent<Collider2D> ();
			SetMovementAuthorization (false);
			if (StartMovingWhenPlayerIsColliding)
			{
				ScriptActivated = true;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance can move.
		/// </summary>
		/// <value><c>true</c> if this instance can move; otherwise, <c>false</c>.</value>
		public override bool CanMove
		{
			get 
			{
				if (OnlyMovesWhenCharacterIsColliding)
				{
					if (!_collidingWithPlayer)
					{
						return false;
					}

					if (_collidingController == null)
					{
						return false;
					}

					// if we're colliding with a character, we check that's it's actually above the platform's top
					_platformTopY = (_collider2D != null) ? _collider2D.bounds.max.y : this.transform.position.y;
					if (_collidingController.ColliderBottomPosition.y < _platformTopY - _toleranceY)
					{
						return false;
					}
                    
					if (OnlyMovesWhenPlayerIsColliding && _collidingController.tag != "Player")
					{
						return false;
					}
				}

				if (OnlyMovesWhenPlayerIsColliding && _collidingController == null)
				{
					return false;
				}

				if (ScriptActivated)
				{
					return _scriptActivatedAuthorization;
				}

				return true;
			}
		}

		public override void MoveAlongThePath()
		{
			base.MoveAlongThePath();
			Physics2D.SyncTransforms();
		}

		protected CorgiController _collidingController = null;
		protected bool _collidingWithPlayer;

		/// <summary>
		/// Sets the movement authorization to true or false based on the status set in parameter
		/// </summary>
		/// <param name="status"></param>
		public virtual void SetMovementAuthorization(bool status)
		{
			_scriptActivatedAuthorization = status;
		}

		/// <summary>
		/// Sets the script authorization to true
		/// </summary>
		public virtual void AuthorizeMovement()
		{
			_scriptActivatedAuthorization = true;
		}

		/// <summary>
		/// Sets the script authorization to false
		/// </summary>
		public virtual void ForbidMovement()
		{
			_scriptActivatedAuthorization = false;
		}

		/// <summary>
		/// Sets the script authorization to true if it was false, false if it was true
		/// </summary>
		public virtual void ToggleMovementAuthorization()
		{
			_scriptActivatedAuthorization = !_scriptActivatedAuthorization;
		}

		/// <summary>
		/// Resets the end reached status, allowing you to move in the opposite direction if CycleOption is set to StopAtBounds
		/// </summary>
		public virtual void ResetEndReached()
		{
			_endReached = false;
		}

		/// <summary>
		/// Forces the moving platform to move back to its start position
		/// </summary>
		public virtual void MoveTowardsStart()
		{
			_endReached = false;
			_direction = -1;
			if (CurrentSpeed.magnitude > 0f)
			{
				_previousPoint = _currentPoint.Current;
				_currentPoint.MoveNext();
			}
			_scriptActivatedAuthorization = true;
		}

		/// <summary>
		/// Forces the moving platform to move to its end position
		/// </summary>
		public virtual void MoveTowardsEnd()
		{
			_endReached = false;
			_direction = 1;
			if (CurrentSpeed.magnitude > 0f)
			{
				_previousPoint = _currentPoint.Current;
				_currentPoint.MoveNext();
			}
			_scriptActivatedAuthorization = true;
		}

		protected override void PointReached()
		{
			base.PointReached();
			PointReachedFeedback?.PlayFeedbacks(this.transform.position);
		}

		protected override void EndReached()
		{
			base.EndReached();
			EndReachedFeedback?.PlayFeedbacks(this.transform.position);
		}

		/// <summary>
		/// When entering collision with something, we check if it's a player, and in that case we set our flag accordingly
		/// </summary>
		/// <param name="collider">Collider.</param>
		public virtual void OnTriggerEnter2D(Collider2D collider)
		{
			CorgiController controller=collider.GetComponent<CorgiController>();
			if (controller==null)
			{
				return;
			}				

			_collidingWithPlayer = true;	
			_collidingController = controller;

			if (StartMovingWhenPlayerIsColliding)
			{
				AuthorizeMovement();
			}
		}

		/// <summary>
		/// When exiting collision with something, we check if it's a player, and in that case we set our flag accordingly
		/// </summary>
		/// <param name="collider">Collider.</param>
		public virtual void OnTriggerExit2D(Collider2D collider)
		{
			CorgiController controller=collider.GetComponent<CorgiController>();
			if (controller==null)
				return;

			_collidingWithPlayer=false;		
			_collidingController = null;	
		}

		/// <summary>
		/// When the player respawns, we reset the position and behaviour of this moving platform
		/// </summary>
		/// <param name="checkpoint">Checkpoint.</param>
		/// <param name="player">Player.</param>
		public virtual void OnPlayerRespawn (CheckPoint checkpoint, Character player)
		{
			if (ResetPositionWhenPlayerRespawns)
			{
				Initialization ();	
			}
		}

		/// <summary>
		/// Catches respawn events and resets the platform's position if needed
		/// </summary>
		/// <param name="eventType"></param>
		public virtual void OnMMEvent(CorgiEngineEvent eventType)
		{
			if (eventType.EventType == CorgiEngineEventTypes.Respawn)
			{
				if (ResetPositionWhenPlayerRespawns)
				{
					Initialization();
				}
			}            
		}

		/// <summary>
		/// On enable, starts listening for CorgiEngine events
		/// </summary>
		protected virtual void OnEnable()
		{
			this.MMEventStartListening<CorgiEngineEvent>();
		}

		/// <summary>
		/// On disable, stops listening for CorgiEngine events
		/// </summary>
		protected virtual void OnDisable()
		{
			this.MMEventStopListening<CorgiEngineEvent>();
		}
	}
}