using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;  
using UnityEngine.UI;  
using UnityEngine.SceneManagement; 

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    private Vector3 velocity;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;
    public float mouseSensitivity = 100f;

    private CharacterController controller;
    public Animator animator;
    public Camera camera;
    public GameObject uiCanvas;
    public GameObject artifactMenu;
    public GameObject artifactInfoCanvas;
    public GameObject questionsMenu;  // The menu for questions
    public GameObject mapCanvas;  // The map UI
    public GameObject helpMenuCanvas;
    public List<string> inventory;  // This will keep track of collected artifacts
    public int maxHealth = 100; // Player's max health
    public int currentHealth; // Player's current health
    public HealthBar healthBar;
    private float xRotation = 0f;

    [Header("View Bobbing Settings")]
    public float bobbingSpeed = 5f;
    public float bobbingAmount = 0.05f;
    private float defaultYPos;
    private float bobbingTimer = 0f;

    // Question Management Variables
    [System.Serializable]
    public class Question
    {
        public string question;
        public string[] answers;
        public int correctAnswerIndex; // Add the correct answer index
    }

    [System.Serializable]
    public class QuestionList
    {
        public Question[] questions;
    }

    public TMP_Text questionText;                  // UI Text to display the question
    public Button[] answerButtons;             // TMP Buttons for answer choices (A, B, C, D)
    private QuestionList questionList;             // Holds all the questions
    private int currentQuestionIndex = 0;          // Tracks the current question
    private int correctAnswersCount = 0;           // Track number of correct answers

    public Button artifactsClose;  // Button to close the artifact menu
    public Button questionsClose;  // Button to close the questions menu
    public Button artifactInfoClose;  // Button to close the artifact info menu
    public Button artifactInfoOpen1; // Button to open the artifact info menu for artifact at position 1
    public Button artifactInfoOpen2;
    public Button artifactInfoOpen3;
    public Button artifactInfoOpen4;
    public TMP_Text artifactInfoName;  // TMP_Text to display the name of the artifact in artifactInfoMenu
    public TMP_Text artifactInfoText;  // TMP_Text to display the description of the artifact in artifactInfoMenu
    public Image artifactInfoImage;  // Image to display the image of the artifact in artifactInfoMenu
    public Sprite LaurelWreathSprite;
    public Sprite MedusaBustSprite;
    public Sprite SpartanHelmetSprite;
    public Sprite PaintedVaseSprite;
    public TextAsset LaurelWreathInfo; // TextAsset to hold the description of the artifact
    public TextAsset MedusaBustInfo;
    public TextAsset SpartanHelmetInfo;
    public TextAsset PaintedVaseInfo;
    public bool allQuestionsAnswered = false;    // Flag to check if all questions have been answered
    private bool allArtifactsCollected = false;   // Flag to check if all artifacts have been collected

    public TMP_Text artifactText;  // TMP_Text to display the list of collected artifacts
    public TMP_Text infoText;      // TMP_Text to display general info/warnings in UI
    public GameObject portal;
    public AudioClip pickUpSound;
    private AudioSource pickUpSource;
    public AudioClip portalSound;
    private AudioSource portalSource; 
    
    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;

        defaultYPos = camera.transform.localPosition.y;

        LoadQuestions();  // Load the questions from the JSON file
        SetUpAnswerButtons(); // Set up the answer buttons with the respective answers

        uiCanvas.SetActive(true); 
        artifactMenu.SetActive(false);
        questionsMenu.SetActive(false);
        artifactInfoCanvas.SetActive(false);
        mapCanvas.SetActive(false);
        helpMenuCanvas.SetActive(false);

        // Add listeners to close the menus when the close buttons are clicked
        artifactsClose.onClick.AddListener(CloseArtifactMenu);
        questionsClose.onClick.AddListener(CloseQuestionsMenu);
        artifactInfoClose.onClick.AddListener(CloseArtifactInfoMenu);
        artifactInfoOpen1.onClick.AddListener(delegate { OpenArtifactInfoMenu(0); });
        artifactInfoOpen2.onClick.AddListener(delegate { OpenArtifactInfoMenu(1); });
        artifactInfoOpen3.onClick.AddListener(delegate { OpenArtifactInfoMenu(2); });
        artifactInfoOpen4.onClick.AddListener(delegate { OpenArtifactInfoMenu(3); });

        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);

        UpdateInfoText("");  // Clear the info text

        portal.SetActive(false); // Hide the portal until all questions are answered

        pickUpSource = GetComponent<AudioSource>();
        if (pickUpSource == null)
        {
            pickUpSource = gameObject.AddComponent<AudioSource>();
        } 
        portalSource = GetComponent<AudioSource>();
        if (portalSource == null)
        {
            portalSource = gameObject.AddComponent<AudioSource>();
        }

    }

    void Update()
    {
        HandleMouseLook();
        HandleMovement();
        HandleJumping();
        HandleGrabbing();
        HandleViewBobbing();
        HandleCursorToggle();
        HandleMapToggle();
        HandleHelpToggle();

        // Death condition
        if (currentHealth <= 0)
        {
            Debug.Log("Player is dead!");
            SceneManager.LoadScene(2); // Load game over / try again scene
        }
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.Rotate(Vector3.up * mouseX);
        camera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    private void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        controller.Move(move * speed * Time.deltaTime);

        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleJumping()
    {
        if (Input.GetKeyDown(KeyCode.Space) && controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            animator.SetTrigger("Jump");
        }
    }

    private void HandleGrabbing()
    {
        if (Input.GetKeyDown(KeyCode.E) && controller.isGrounded)
        {
            
            // If there is a raycast hit and the object is grabbable and within 5f
            RaycastHit hit;
            if (Physics.Raycast(camera.transform.position, camera.transform.forward, out hit, 5f))
            {
                if (hit.collider.gameObject.tag == "Artifact")
                {
                    animator.SetTrigger("Grab");
                    // Hide the artifact
                    hit.collider.gameObject.SetActive(false);  
                    // Add the hit object to the player's inventory
                    inventory.Add(hit.collider.gameObject.name);
                    // Update the artifact list in the UI
                    UpdateArtifactUI();
                    if (pickUpSound != null && pickUpSource != null)
                    {
                        pickUpSource.PlayOneShot(pickUpSound);
                    }
                }
                if (hit.collider.gameObject.tag == "Questioner")
                {
                    if (!allQuestionsAnswered && allArtifactsCollected) {
                        animator.SetTrigger("Grab");
                        // Enable the question menu and load the first question
                        questionsMenu.SetActive(true);
                        Cursor.lockState = CursorLockMode.None;
                        Cursor.visible = true;
                        correctAnswersCount = 0; // Reset the correct answers count
                        currentQuestionIndex = 0; // Reset the question index
                        UpdateInfoText(""); // Clear the info text
                        DisplayQuestion();  // Show the first question when the menu is enabled
                    } else if (!allArtifactsCollected){
                        UpdateInfoText("You need to collect all artifacts to proceed to the questions.");
                    } else {
                        UpdateInfoText("You have already answered all questions. Proceed through the portal.");
                    }
                }
            }
        }
    }

    // Toggle curson visability and lock state on C
    private void HandleCursorToggle()
    { 
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            } else {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    private void HandleMapToggle()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            Debug.Log("Map toggled");
            // Toggle the map on and off
            if (mapCanvas.activeSelf)
            {
                mapCanvas.SetActive(false);
                uiCanvas.SetActive(true);
            } else {
                mapCanvas.SetActive(true);
                uiCanvas.SetActive(false);
                artifactInfoCanvas.SetActive(false);
            }
        }
    }

    private void HandleHelpToggle()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            if (helpMenuCanvas.activeSelf)
            {
                helpMenuCanvas.SetActive(false);
                uiCanvas.SetActive(true);
            } else {
                helpMenuCanvas.SetActive(true);
                uiCanvas.SetActive(false);
                artifactInfoCanvas.SetActive(false);
                mapCanvas.SetActive(false);
            }
        }
    }

    private void HandleViewBobbing()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        if (Mathf.Abs(moveX) > 0.1f || Mathf.Abs(moveZ) > 0.1f)
        {
            bobbingTimer += Time.deltaTime * bobbingSpeed;
            float newY = defaultYPos + Mathf.Sin(bobbingTimer) * bobbingAmount;

            camera.transform.localPosition = new Vector3(
                camera.transform.localPosition.x,
                newY,
                camera.transform.localPosition.z
            );
        }
        else
        {
            bobbingTimer = 0f;
            camera.transform.localPosition = new Vector3(
                camera.transform.localPosition.x,
                Mathf.Lerp(camera.transform.localPosition.y, defaultYPos, Time.deltaTime * 5f),
                camera.transform.localPosition.z
            );
        }
    }

    // Call when the player takes damage
    public void DamagePlayer( int damage )
    {
        currentHealth -= damage;
        healthBar.SetHealth(currentHealth);
    }

    // Damage play upon collision with enemy/obsticle
    // Enemy object must have a collider with "Is Trigger" checked and tag "Enemy"
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Enemy")
        {
            DamagePlayer(10);
        }

    }

    // Question Manager Methods
    void LoadQuestions()
    {
        // Load the JSON file from the Resources folder
        TextAsset json = Resources.Load<TextAsset>("questions"); // Make sure the file is named "questions.json"

        if (json != null)
        {
            // Deserialize the JSON into a temporary QuestionList object
            QuestionList loadedQuestions = JsonUtility.FromJson<QuestionList>(json.text);

            if (loadedQuestions != null && loadedQuestions.questions.Length > 0)
            {
                questionList = new QuestionList();
                questionList.questions = new Question[loadedQuestions.questions.Length];

                for (int i = 0; i < loadedQuestions.questions.Length; i++)
                {
                    // Create a new Question object and copy data from the loaded question
                    Question newQuestion = new Question
                    {
                        question = loadedQuestions.questions[i].question,
                        answers = loadedQuestions.questions[i].answers,
                        correctAnswerIndex = loadedQuestions.questions[i].correctAnswerIndex
                    };

                    // Add the new Question object to the questionList
                    questionList.questions[i] = newQuestion;
                }

                Debug.Log("Questions loaded successfully.");
            }
            else
            {
                Debug.LogError("No questions found in the loaded JSON.");
            }
        }
        else
        {
            Debug.LogError("Failed to load questions from JSON.");
        }
    }

    void SetUpAnswerButtons()
    {
        // Ensure each TMP button's text field is updated with the answers from the current question
        foreach (Button button in answerButtons)
        {
            button.onClick.AddListener(() => VerifyAnswer(button));
        }
    }

    void DisplayQuestion()
    {
        // Check if there are questions to display
        if (questionList != null && questionList.questions.Length > 0)
        {
            Question currentQuestion = questionList.questions[currentQuestionIndex];
            questionText.text = currentQuestion.question;

            // Display answers
            for (int i = 0; i < currentQuestion.answers.Length; i++)
            {
                if (i < answerButtons.Length)
                {
                    // Set the text for each TMP button to the corresponding answer
                    TMP_Text buttonText = answerButtons[i].GetComponentInChildren<TMP_Text>();
                    if (buttonText != null)
                    {
                        buttonText.text = currentQuestion.answers[i];
                    }
                }
            }
        }
        else
        {
            Debug.LogError("No questions available to display.");
        }
    }

    // This method is triggered when any TMP answer button is clicked
    public void VerifyAnswer(Button selectedButton)
    {
        Question currentQuestion = questionList.questions[currentQuestionIndex];
        int selectedAnswerIndex = System.Array.IndexOf(answerButtons, selectedButton); // Get the index of the selected answer

        // Check if the selected answer is correct
        if (selectedAnswerIndex == currentQuestion.correctAnswerIndex)
        {
            UpdateInfoText("Correct answer!");
            correctAnswersCount++; // Increment the correct answer count
        }
        else
        {
            UpdateInfoText("Wrong answer! The correct answer was: " + currentQuestion.answers[currentQuestion.correctAnswerIndex]);
        }

        // After answering, go to next question
        currentQuestionIndex += 1;

        // If all questions have been answered, notify the player
        if (currentQuestionIndex % questionList.questions.Length == 0)
        {
            allQuestionsAnswered = true;
            string infoString = "Questions for this level are done! You got " + correctAnswersCount + " correct answers.";
            UpdateInfoText(infoString);
            CloseQuestionsMenu();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Check if all questions are answered correctly
            if (correctAnswersCount == questionList.questions.Length)
            {
                UpdateInfoText(infoString + "\n" + "All questions answered correctly, you may proceed through the portal");
                portal.SetActive(true); // Enable the portal
                if (portalSound != null && portalSource != null)
                {
                    portalSource.PlayOneShot(portalSound);
                }
            } else {
                UpdateInfoText(infoString + "\n" + "You need to answer all questions correctly to proceed through the portal. Try again!");
                allQuestionsAnswered = false;
            }
        } else {
            DisplayQuestion(); // Display the next question
        }
    }

    // Method to close the artifact menu
    void CloseArtifactMenu()
    {
        artifactMenu.SetActive(false);
    }

    // Method to close the questions menu
    void CloseQuestionsMenu()
    {
        questionsMenu.SetActive(false);
    }

    // Method to close the artifact info menu, open the artifact menu
    void CloseArtifactInfoMenu()
    {
        artifactInfoCanvas.SetActive(false);
        uiCanvas.SetActive(true);
        artifactMenu.SetActive(true);
        questionsMenu.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Method to open the artifact info menu
    void OpenArtifactInfoMenu(int n)
    {
        // Button can only be associated with possition in list (ex: button 1 = artifact in list position 1)
        // We need to take the button input "n" and pull the artifact name in the list "inventory" at position "n"
        artifactInfoCanvas.SetActive(true);
        uiCanvas.SetActive(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        string artifactName = inventory[n];
        artifactInfoName.text = artifactName;

        // Images and descriptions for each artifact
        if (artifactName == "Laurel Wreath")
        {
            artifactInfoImage.sprite = LaurelWreathSprite;
            artifactInfoText.text = LaurelWreathInfo.text;
        }
        else if (artifactName == "Medusa Bust")
        {
            artifactInfoImage.sprite = MedusaBustSprite;
            artifactInfoText.text = MedusaBustInfo.text;
        }
        else if (artifactName == "Spartan Helmet")
        {
            artifactInfoImage.sprite = SpartanHelmetSprite;
            artifactInfoText.text = SpartanHelmetInfo.text;
        }
        else if (artifactName == "Painted Vase")
        {
            artifactInfoImage.sprite = PaintedVaseSprite;
            artifactInfoText.text = PaintedVaseInfo.text;
        }


        
    }

    // Method to update the artifact UI
    void UpdateArtifactUI()
    {
        artifactMenu.SetActive(true);
        // Clear current text
        artifactText.text = "Artifacts: " + "\n";
        // Display all collected artifacts
        foreach (string artifact in inventory)
        {
            artifactText.text += artifact + "\n";
            artifactInfoOpen1.gameObject.SetActive(true);
        }

        // If all artifacts are collected, display a completion message
        if (inventory.Count >= 4)
        {
            artifactText.text += "\nAll artifacts collected!";
            allArtifactsCollected = true;
        }

        // Set up buttons to open artifact info menu
        artifactInfoOpen1.gameObject.SetActive(false);
        artifactInfoOpen2.gameObject.SetActive(false);
        artifactInfoOpen3.gameObject.SetActive(false);
        artifactInfoOpen4.gameObject.SetActive(false);
        if (inventory.Count >= 4) {
            artifactInfoOpen1.gameObject.SetActive(true);
            artifactInfoOpen2.gameObject.SetActive(true);
            artifactInfoOpen3.gameObject.SetActive(true);
            artifactInfoOpen4.gameObject.SetActive(true);
        } else if (inventory.Count == 3) {
            artifactInfoOpen1.gameObject.SetActive(true);
            artifactInfoOpen2.gameObject.SetActive(true);
            artifactInfoOpen3.gameObject.SetActive(true);
        } else if (inventory.Count == 2) {
            artifactInfoOpen1.gameObject.SetActive(true);
            artifactInfoOpen2.gameObject.SetActive(true);
        } else if (inventory.Count == 1) {
            artifactInfoOpen1.gameObject.SetActive(true);
        }
    }

    void UpdateInfoText(string t)
    {
        infoText.text = t;
    }
}