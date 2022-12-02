using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class RubyController : MonoBehaviour
{
    public float speed = 3.0f;

    public int maxHealth = 5;

    public AudioClip throwSound;
    public AudioClip hitSound;
    public AudioClip winSound;
    public AudioClip loseSound;
    public AudioClip talkSound;

    public AudioClip backgroundSound;

    public int health { get { return currentHealth; } }
    int currentHealth;

    public float timeInvincible = 2.0f;
    bool isInvincible;
    float invincibleTimer;

    public float timeBoosting = 4.0f;
    float speedBoostTimer;
    bool isBoosting;

    Rigidbody2D rigidbody2d;
    float horizontal;
    float vertical;

    // Health and Damage Particles
    public ParticleSystem healthEffect;
    public ParticleSystem damageEffect;

    // Fixed Robots TMP Integers
    public TextMeshProUGUI fixedText;
    private int scoreFixed = 0;

    // Win text
    public GameObject WinTextObject;

    //Lose text
    public GameObject LoseTextObject;
    bool gameOver;

    Animator animator;
    Vector2 lookDirection = new Vector2(1, 0);

    AudioSource audioSource;

    // Cog Object and Ammo Variables
    public GameObject projectilePrefab;
    public int ammo { get { return currentAmmo; } }
    public int currentAmmo;

    // Ammo Text UI
    public TextMeshProUGUI ammoText;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        currentHealth = maxHealth;

        audioSource = GetComponent<AudioSource>();

        // Fixed Robot Text
        fixedText.text = "Fixed Robots: " + scoreFixed.ToString() + "/6";

        // Win Text
        WinTextObject.SetActive(false);
        LoseTextObject.SetActive(false);
        gameOver = false;

        //Background music
        audioSource.clip = backgroundSound;
        audioSource.loop = true;
        audioSource.Play();


        // Ammo at start
        rigidbody2d = GetComponent<Rigidbody2D>();
        AmmoText();
    }

    // Update is called once per frame
    void Update()
    {
        if (isBoosting == true)
        {

            speed = speed + 0.001f;
        }

        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        Vector2 move = new Vector2(horizontal, vertical);

        if (!Mathf.Approximately(move.x, 0.0f) || !Mathf.Approximately(move.y, 0.0f))
        {
            lookDirection.Set(move.x, move.y);
            lookDirection.Normalize();
        }

        animator.SetFloat("Look X", lookDirection.x);
        animator.SetFloat("Look Y", lookDirection.y);
        animator.SetFloat("Speed", move.magnitude);

        if (isInvincible)
        {
            invincibleTimer -= Time.deltaTime;
            if (invincibleTimer < 0)
                isInvincible = false;
        }

        // Cog Bullet is launched - Ammo in UI is reduced
        if (Input.GetKeyDown(KeyCode.C))
        {
            Launch();

            if (currentAmmo > 0)
            {
                ChangeAmmo(-1);
                AmmoText();
            }
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            RaycastHit2D hit = Physics2D.Raycast(rigidbody2d.position + Vector2.up * 0.2f, lookDirection, 1.5f, LayerMask.GetMask("NPC"));
            if (hit.collider != null)
            {
                NonPlayerCharacter character = hit.collider.GetComponent<NonPlayerCharacter>();
                if (character != null)
                {
                    if (scoreFixed >= 6)
                    {
                        SceneManager.LoadScene("Level 2");
                    }

                    else
                    {
                        character.DisplayDialog();
                        audioSource.clip = talkSound;
                        GetComponent<AudioSource>().PlayOneShot(talkSound);

                        audioSource.clip = backgroundSound;
                        audioSource.Play();
                        transform.position = new Vector3(-8.74f, -4.74f, 0f);

                    }
                }
            }
        }
        // Close Game
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
        // Restart the game
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (gameOver == true)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // this loads the currently active scene
            }
        }
    }

    void FixedUpdate()
    {
        Vector2 position = rigidbody2d.position;
        position.x = position.x + speed * horizontal * Time.deltaTime;
        position.y = position.y + speed * vertical * Time.deltaTime;

        rigidbody2d.MovePosition(position);
    }

    public void ChangeHealth(int amount)
    {
        if (amount < 0)
        {
            if (isInvincible)
                return;

            isInvincible = true;
            invincibleTimer = timeInvincible;

            PlaySound(hitSound);
            damageEffect = Instantiate(damageEffect, rigidbody2d.position + Vector2.up * 0.5f, Quaternion.identity);

            animator.SetTrigger("Hit");
        }

        // When Ruby Gains Health - Particles appear
        if (amount > 0)
        {
            healthEffect = Instantiate(healthEffect, rigidbody2d.position + Vector2.up * 0.5f, Quaternion.identity);
        }
        // Ruby loses all health, lose text appears and restart becomes true
        if (currentHealth <= 0)
        {
            LoseTextObject.SetActive(true);

            audioSource.clip = backgroundSound;
            audioSource.Stop();

            audioSource.clip = loseSound;
            audioSource.Play();

            transform.position = new Vector3(-5f, 0f, -100f);
            speed = 0;
            Destroy(gameObject.GetComponent<SpriteRenderer>());

            gameOver = true;
        }

        // Health math code
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        Debug.Log(currentHealth + "/" + maxHealth);

        UIHealthBar.instance.SetValue(currentHealth / (float)maxHealth);
    }
    // Ammo Function
    public void ChangeAmmo(int amount)
    {
        // Ammo math code
        currentAmmo = Mathf.Abs(currentAmmo + amount);
        Debug.Log("Ammo: " + currentAmmo);
    }

    public void AmmoText()
    {
        ammoText.text = "Ammo: " + currentAmmo.ToString();
    }
    void Launch()
    {
        GameObject projectileObject = Instantiate(projectilePrefab, rigidbody2d.position + Vector2.up * 0.5f, Quaternion.identity);

        Projectile projectile = projectileObject.GetComponent<Projectile>();
        projectile.Launch(lookDirection, 300);

        animator.SetTrigger("Launch");

        PlaySound(throwSound);
    }
    // Plays sounds from this script and others
    public void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }

    public void FixedRobots(int amount)
    {
        scoreFixed += amount;
        fixedText.text = "Fixed Robots: " + scoreFixed.ToString() + "/6";

        Debug.Log("Fixed Robots: " + scoreFixed);

        //Talk to Jambi to visit Stage 2
        if (scoreFixed == 6)
        {
            WinTextObject.SetActive(true);
        }

        // Win Text Appears ONLY if on Level 2
        if (scoreFixed == 6)
        {
            WinTextObject.SetActive(true);

            audioSource.clip = backgroundSound;
            audioSource.Stop();

            audioSource.clip = winSound;
            audioSource.Play();

            gameOver = true;
        }
    }

    public void SpeedBoost(int amount)
    {
        if (amount > 0)
        {
            speedBoostTimer = timeBoosting;
            isBoosting = true;
        }
    }
}

