using UnityEngine;
using UnityEngine.UI;
using TMPro;  


public class QuestionManager : MonoBehaviour
{
    [System.Serializable]
    public class Question
    {
        public string question;
        public string[] answers;
    }

    [System.Serializable]
    public class QuestionList
    {
        public Question[] questions;
    }

    public TMP_Text questionText;                  // UI Text to display the question
    public TMP_Text[] answerTextFields;            // UI Text elements to display the answer choices
    public Button nextButton;                  // Button to go to the next question
    private QuestionList questionList;         // Holds all the questions
    private int currentQuestionIndex = 0;      // Tracks the current question

    void Start()
    {
        LoadQuestions();                       // Load the questions from the JSON file
        DisplayQuestion();                     // Display the first question
        nextButton.onClick.AddListener(NextQuestion); // Add listener for Next button
    }

    void LoadQuestions()
    {
        // Load the JSON file from Resources folder
        TextAsset json = Resources.Load<TextAsset>("questions");  // Ensure your file is named "questions.json" and placed in Resources
        if (json != null)
        {
            questionList = JsonUtility.FromJson<QuestionList>(json.ToString());
        }
        else
        {
            Debug.LogError("Failed to load questions from JSON.");
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
                if (i < answerTextFields.Length)
                {
                    answerTextFields[i].text = currentQuestion.answers[i];
                }
            }
        }
        else
        {
            Debug.LogError("No questions available to display.");
        }
    }

    void NextQuestion()
    {
        // Move to the next question, wrapping around if needed
        currentQuestionIndex = (currentQuestionIndex + 1) % questionList.questions.Length;
        DisplayQuestion();
    }
}
