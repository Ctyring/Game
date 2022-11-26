using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{	
	/// <summary>
	/// Adds this class to your ladders so a Character can climb them.
	/// </summary>
	[AddComponentMenu("Corgi Engine/Environment/Ladder")]
	public class Ladder : MonoBehaviour 
	{
		/// the different types of ladders
		public enum LadderTypes { Simple, BiDirectional }

		[Header("Behaviour")]

		/// determines whether this ladder is simple (vertical) or bidirectional
		[Tooltip("determines whether this ladder is simple (vertical) or bidirectional")]
		public LadderTypes LadderType = LadderTypes.Simple;
		/// should the character be centered horizontally on the ladder when climbing
		[Tooltip("should the character be centered horizontally on the ladder when climbing")]
		public bool CenterCharacterOnLadder = true;

		[Header("Ladder Platform")]

		/// the platform at the top of the ladder - this can be a ground platform 
		[Tooltip("the platform at the top of the ladder - this can be a ground platform ")]
		public GameObject LadderPlatform;
		/// if this is set to true, on Initialization, the LadderPlatform will be automatically repositioned to match the top of the ladder's collider
		[Tooltip("if this is set to true, on Initialization, the LadderPlatform will be automatically repositioned to match the top of the ladder's collider")]
		public bool AutoPositionLadderPlatform = false;

		public BoxCollider2D LadderPlatformBoxCollider2D { get; protected set; }
		public EdgeCollider2D LadderPlatformEdgeCollider2D { get; protected set; }

		protected Collider2D _collider2D;
		protected Vector3 _newLadderPlatformPosition;

		/// <summary>
		/// On Start we initialize our ladder
		/// </summary>
		protected virtual void Start()
		{
			Initialization ();
		}

		/// <summary>
		/// Grabs and stores the collider, and makes sure there's one. Repositions the platform if needed
		/// </summary>
		protected virtual void Initialization()
		{
			_collider2D = GetComponent<Collider2D>();

			if (LadderPlatform == null)
			{
				return;
			}

			LadderPlatformBoxCollider2D = LadderPlatform.GetComponent<BoxCollider2D>();
			LadderPlatformEdgeCollider2D = LadderPlatform.GetComponent<EdgeCollider2D>();

			if (LadderPlatformBoxCollider2D == null && LadderPlatformEdgeCollider2D == null)
			{
				Debug.LogWarning(this.name+" : this ladder's LadderPlatform is missing a BoxCollider2D or an EdgeCollider2D.");
			}

			if (AutoPositionLadderPlatform)
			{
				RepositionLadderPlatform ();
			}
		}

		/// <summary>
		/// Repositions the ladder platform so it matches perfectly the top of the ladder
		/// </summary>
		protected virtual void RepositionLadderPlatform ()
		{
			if (_collider2D == null)
			{
				return;
			}
			if (LadderPlatformBoxCollider2D == null && LadderPlatformEdgeCollider2D == null)
			{
				return;
			}

			if (LadderPlatformBoxCollider2D != null)
			{
				_newLadderPlatformPosition = LadderPlatformBoxCollider2D.transform.localPosition;
				_newLadderPlatformPosition.x = 0;
				_newLadderPlatformPosition.y = _collider2D.bounds.size.y/2 - LadderPlatformBoxCollider2D.bounds.size.y/2;
				_newLadderPlatformPosition.z = this.transform.position.z;
				LadderPlatformBoxCollider2D.transform.localPosition = _newLadderPlatformPosition;
			}

			if (LadderPlatformEdgeCollider2D != null)
			{
				_newLadderPlatformPosition = LadderPlatformEdgeCollider2D.transform.localPosition;
				_newLadderPlatformPosition.x = 0;
				_newLadderPlatformPosition.y = _collider2D.bounds.size.y/2 - LadderPlatformBoxCollider2D.bounds.size.y/2;
				_newLadderPlatformPosition.z = this.transform.position.z;
				LadderPlatformEdgeCollider2D.transform.localPosition = _newLadderPlatformPosition;
			}
		}

		/// <summary>
		/// Triggered when something collides with the ladder
		/// </summary>
		/// <param name="collider">Something colliding with the ladder.</param>
		protected virtual void OnTriggerEnter2D(Collider2D collider)
		{
			// we check that the object colliding with the ladder is actually a corgi controller and a character
			CharacterLadder characterLadder = collider.gameObject.MMGetComponentNoAlloc<Character>()?.FindAbility<CharacterLadder>();
			if (characterLadder==null)
			{
				return;					
			}

			characterLadder.AddCollidingLadder(_collider2D);
		}

		/// <summary>
		/// Triggered when something exits the ladder
		/// </summary>
		/// <param name="collider">Something colliding with the ladder.</param>
		protected virtual void OnTriggerExit2D(Collider2D collider)
		{
			// we check that the object colliding with the ladder is actually a corgi controller and a character
			CharacterLadder characterLadder = collider.gameObject.MMGetComponentNoAlloc<Character>()?.FindAbility<CharacterLadder>();
			if (characterLadder==null)
			{
				return;					
			}
			characterLadder.RemoveCollidingLadder(_collider2D);		
		}
	}
}