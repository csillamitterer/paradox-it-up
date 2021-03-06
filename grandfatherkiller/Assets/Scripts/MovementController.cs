using UnityEngine;
using System.Collections;
using InControl;

[RequireComponent(typeof(NavMeshAgent))]
public class MovementController : MonoBehaviour {
	public float maxSpeed = 5.0f;
	public int playerNumber = 1;

	private Vector3 moveDirection = Vector3.zero;
	private Vector3 facingDirection = Vector3.left;

	public GameObject shot;
	public GameObject slash;
	public GameObject bloodSplatter;
	public Transform shotSpawn;
	public Transform meleeSpawn;
	public float fireTime = 0.5f;
	public float meleeTime = 0.5f;
	public int maxHealth = 2;
	public float respawnTime = 2.0f;
	public AudioClip sfxShot;
	public AudioClip sfxMelee;
	
	private float nextAttack;
	private int health;
	private bool dead;



	Vector3 initialPosition;
	Quaternion initialRotation;




	// Use this for initialization
	void Start () {
		health = maxHealth;
		dead = false;

		initialPosition = gameObject.transform.position;
		initialRotation = gameObject.transform.rotation;

		PlayerPrefs.SetInt ("player1CivKills", 0);
		PlayerPrefs.SetInt ("player2CivKills", 0);
	}
	
	// Update is called once per frame
	void Update () {
		NavMeshAgent controller = GetComponent<NavMeshAgent>();

		var inputDevice = (InputManager.Devices.Count + 1 > playerNumber) ? InputManager.Devices[playerNumber - 1] : null;
		if (inputDevice != null) {
			moveDirection = Vector3.Normalize(new Vector3(inputDevice.Direction.X, 0, inputDevice.Direction.Y));
			if (inputDevice.RightStick.X != 0 || inputDevice.RightStick.Y != 0) {
				facingDirection = Vector3.Normalize(new Vector3(inputDevice.RightStick.X, 0, inputDevice.RightStick.Y));
			} else if (moveDirection != Vector3.zero) {
				facingDirection = moveDirection;
			}
			// moveDirection = transform.TransformDirection(moveDirection);


			// Firing
			if ((inputDevice.Action4.IsPressed || inputDevice.Action3.IsPressed)&& Time.time > nextAttack)
			{	
				nextAttack = Time.time + fireTime;
				GameObject zBullet = (GameObject)Instantiate (shot, shotSpawn.position, shotSpawn.rotation);
				zBullet.GetComponent<ShotController> ().SetVelocity ();
				zBullet.GetComponent<ShotController> ().playerNumber = playerNumber;
				AudioSource.PlayClipAtPoint (sfxShot, shotSpawn.position, 1.0f);
			}

			// Melee
			if ((inputDevice.Action2.WasPressed || inputDevice.Action1.IsPressed) && Time.time > nextAttack) {
				nextAttack = Time.time + meleeTime;
				GameObject zSlash = (GameObject)Instantiate (slash, meleeSpawn.position, meleeSpawn.rotation);
				zSlash.transform.Rotate(transform.up * -90.0f);
				zSlash.GetComponent<SlashController> ().SetAngularVelocity ();
				zSlash.GetComponent<SlashController> ().playerNumber = playerNumber;
				AudioSource.PlayClipAtPoint (sfxMelee, shotSpawn.position, 1.0f);
				zSlash.transform.parent = transform;

			}
		}
		controller.Move(((moveDirection * Time.deltaTime)/(1+(moveDirection - facingDirection).magnitude))* maxSpeed);
		transform.forward = facingDirection;

		if (dead) {
			die ();
		}
	}

	void OnTriggerEnter (Collider other)
	{
		if (dead)
			return;
		if (other.tag == "Shot") {
			if (other.transform.parent.gameObject.GetComponent<ShotController>().playerNumber != playerNumber) {
				health -= 1;
				Vector3 bloodSplatterPos = transform.position;
				Quaternion bloodSplatterRotation = Quaternion.Euler(90, Random.Range(0, 360), 0);
				bloodSplatterPos.y = 0.01f;
				GameObject zBlood = (GameObject)Instantiate (bloodSplatter, bloodSplatterPos, bloodSplatterRotation);
				if (health <= 0) {
					dead = true;
				}
				Destroy(other.transform.parent.gameObject);
			}

		} else if (other.tag == "Slash") {
			if (other.transform.parent.gameObject.GetComponent<SlashController>().playerNumber != playerNumber) {
				health -= 1;
				Vector3 bloodSplatterPos = transform.position;
				Quaternion bloodSplatterRotation = Quaternion.Euler( 90, Random.Range(0, 360), 0);
				bloodSplatterPos.y = 0.01f;
				GameObject zBlood = (GameObject)Instantiate (bloodSplatter, bloodSplatterPos, bloodSplatterRotation);
				if (health <= 0) {
					dead = true;
				}
			}
		} 
	}

	void die(){
		gameObject.SetActive (false);
		Invoke ("respawn", respawnTime);
	}

	void respawn(){
		gameObject.SetActive (true);
		dead = false;
		health = maxHealth;


		gameObject.transform.position = initialPosition;
		gameObject.transform.rotation = initialRotation;
	}
}
