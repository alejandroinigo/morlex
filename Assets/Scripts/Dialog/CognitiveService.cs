using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CognitiveService : ScriptableObject
{
    string lastUserInput = "";
    const string repeatedResponseMessage = "Please don't repeat yourself";

    public Tuple<string, float> GetCognitiveResponse(string userInput) {
        float score = 0f;
        if (lastUserInput.Equals(userInput)) {
            return Tuple.Create(repeatedResponseMessage, score);
        } else {
            lastUserInput = userInput;
            Tuple<KeywordReply, string> keywordData = FindKeyword(NormalizeInput(userInput));
            if (NoMatchKeywordReply.Keywords.Equals(keywordData.Item1.Keywords)) {
                score = 0f;
            } else {
                score = GetEntropy(userInput);
            }
            return Tuple.Create(keywordData.Item1.FormatReply(keywordData.Item2), score);
        }
    }
    
    float GetEntropy(string input) {
        float entropy = 0;
        Dictionary<char, float> table = new Dictionary<char, float>();

        foreach (char c in input) {
            if (table.ContainsKey(c))
                table[c]++;
            else
                table.Add(c, 1);

        }
        float freq;
        foreach (KeyValuePair<char, float> letter in table) {
            freq = letter.Value / input.Length;
            entropy += freq * (float)(Math.Log(freq) / Math.Log(2));
        }
        entropy *= -1;
        return entropy;
    }
    readonly List<KeywordReply> keywordreplies = new List<KeywordReply> {
        new KeywordReply() {
            Keywords = { "can you" },
            Replies = {
                "Don't you believe that I can*?",
                "Perhaps you would like to be able to*",
                "You want me to be able to*",
            }
        },
        new KeywordReply() {
            Keywords = { "can i" },
            Replies = {
                "Perhaps you want to*",
                "Do you want to be able to*?",
            }
        },
        new KeywordReply() {
            Keywords = { "morlex" },
            Replies = {
                "Whatever starts up must pass away. Why do you mention Morlex?",
                "Everything circles around. What is it about Morlex that worries you?",
                "Things just repeat. What do you think Morlex has to do with your problem?",
                "Everything’s empty. Are you frightened by Morlex?",
                "Nothing is real. Nothing matters",
            }
        },
        new KeywordReply() {
            Keywords = { "you are", "youre" },
            Replies = {
                "What makes you think I am*?",
                "Does it please you to believe that I am*?",
                "Perhaps you would like to be*",
                "Do you sometimes wish you were*?",
            }
        },
        new KeywordReply() {
            Keywords = { "i dont" },
            Replies = {
                "Don't you really*?",
                "Why don't you*?",
                "Do you wish to be able to*?",
                "Does that trouble you?",
            }
        },
        new KeywordReply() {
            Keywords = { "i feel" },
            Replies = {
                "Tell me more about such feeling",
                "Do you often feel*?",
                "Do you enjoy feeling*?",
            }
        },
        new KeywordReply() {
            Keywords = { "why dont you" },
            Replies = {
                "Do you really believe I don't*?",
                "Perhaps in good time I will*",
                "Do you want me to*?",
            }
        },
        new KeywordReply() {
            Keywords = { "why cant i" },
            Replies = {
                "Do you think you should be able to*?",
                "Why can't you*?",
            }
        },
        new KeywordReply() {
            Keywords = { "are you" },
            Replies = {
                "Why are you interested in whether or not I am*?",
                "Would you prefer if I were not*?",
                "Perhaps in your fantasies I am*",
            }
        },
        new KeywordReply() {
            Keywords = { "i cant" },
            Replies = {
                "How do you know you can't*?",
                "Have you tried?",
                "Perhaps you can now*",
            }
        },
        new KeywordReply() {
            Keywords = { "sad", "unhappy" },
            Replies = {
                "I am sorry to hear that you are sad",
                "Do you think coming here will help you not to be sad?",
                "I'm sure it's not pleasant to be sad",
                "Can you explain what made you sad?",
            }
        },
        new KeywordReply() {
            Keywords = { "i am", "im" },
            Replies = {
                "Did you come to me because you are*?",
                "Can you explain why you are*?",
                "What makes you*?",
                "Do you believe it is normal to be*?",
                "Do you enjoy being*?",
            }
        },
        new KeywordReply() {
            Keywords = { "i want" },
            Replies = {
                "What would it mean if you got*?",
                "Why do you want*?",
                "Suppose you soon got*",
                "What if you never got*",
                "I sometimes also want*",
            }
        },
        new KeywordReply() {
            Keywords = { "i like", "i love" },
            Replies = {
                "Why do you like*?",
                "Do you often think of*?",
                "I sometimes also like*",
            }
        },
        new KeywordReply() {
            Keywords = { "what", "how", "who", "where", "when", "why" },
            Replies = {
                "Why do you ask?",
                "Does that question interest you?",
                "What answer would please you the most?",
                "What do you think?",
                "Are such questions or your mind often?",
                "What is it you really want to know?",
                "Have you asked anyone else?",
                "Have you asked such question before?",
                "What else comes to mind when you ask that?",
            }
        },
        new KeywordReply() {
            Keywords = { "name" },
            Replies = {
                "Names don't interest me",
                "I don't care about names. Go on",
            }
        },
        new KeywordReply() {
            Keywords = { "because", "cause" },
            Replies = {
                "Is that the real reason?",
                "Don't any other reasons come to mind?",
                "Does that reason explain anything else?",
                "What other reason migth there be?",
            }
        },
        new KeywordReply() {
                Keywords = { "sorry", "apologise" },
                Replies = {
                    "Please don't apologize",
                    "Apologies are not necessary",
                    "What feeling do you get when you apologize?",
                    "It did not bother me. Please continue"
                }
            },
        new KeywordReply() {
                Keywords = { "remember" },
                Replies = {
                    "Do you often think of*?",
                    "What else do you recollect*?",
                    "What in the present situation reminds you of*?",
                }
            },
        new KeywordReply() {
            Keywords = { "dream" },
            Replies = {
                "What does that dream suggest to you?",
                "Do you dream often?",
                "What persons appear in your dreams?",
                "Are you disturbed by your dreams?",
            }
        },
        new KeywordReply() {
            Keywords = { "hello", "hi" },
            Replies = {
                "Welcome",
                "Hi",
                "Hello",
            }
        },
        new KeywordReply() {
            Keywords = { "maybe", "perhaps" },
            Replies = {
                "You don't seem quite certain",
                "Why the uncertain tone?",
                "Can't you be more positive?",
                "You aren't sure?",
                "Don't you know?",
            }
        },
        new KeywordReply() {
            Keywords = { "you" },
            Replies = {
                "We were discussing you, not me",
                "Oh, I*",
                "You're not really talking about me, are you?",
            }
        },
        new KeywordReply() {
            Keywords = { "your" },
            Replies = {
                "Why are you concerned over my*?",
                "What about your own*?",
                "Are you worried about someone else's*?",
                "Really, my*?",
                "What makes you think of my*?",
                "Do you want my*?"
            }
        },
        new KeywordReply() {
            Keywords = { "everyone", "everybody", "nobody" },
            Replies = {
                "Can you think of anyone in particular?",
                "Who, for example?",
                "Are you thinking of a very special person?",
                "Who, may I ask?",
                "Someone special perhaps?",
                "You have a particular person in mind, don't you?",
                "Who do you think you're talking about?",
            }
        },
        new KeywordReply() {
            Keywords = { "always" },
            Replies = {
                "Can you think of a specific example?",
                "When?",
                "Really, always?",
            }
        },
        new KeywordReply() {
            Keywords = { "think", "wish", "believe" },
            Replies = {
                "What are you thinking of?",
                "Really, always?",
                "Do you really think so?",
                "But you are not sure you*",
                "Do you doubt*?",
                "In what way?",
            }
        },
        new KeywordReply() {
            Keywords = { "alike", "is like", "are like", "equivalent", "identical" },
            Replies = {
                "What resemblance do you see?",
                "What does the similarity suggest to you?",
                "What other connections do you see?",
                "Could there really be some connection?",
                "How?",
            }
        },
        new KeywordReply() {
            Keywords = { "different" },
            Replies = {
                "How is it different?",
                "What differences do you see?",
                "What does that difference suggest to you?",
                "What other distinctions do you see?",
                "What do you suppose that disparity means?",
                "Could there be some connection, do you suppose?",
                "How?"
            }
        },
        new KeywordReply() {
            Keywords = { "no", "negative" },
            Replies = {
                "Are you just saying that just to be negative?",
                "You are being a bit negative",
            }
        },
        new KeywordReply() {
            Keywords = { "yes", "certainly", "sure" },
            Replies = {
                "You seem quite positive",
                "Are you sure?",
                "I see",
                "I understand",
            }
        },
        new KeywordReply() {
            Keywords = { "friend" },
            Replies = {
                "Why do you bring up the topic of friends?",
                "Do your friends worry you?",
                "Do your friends pick on you?",
                "Are you sure you have any friends?",
                "Do you impose on your friends?",
                "Perhaps your love for friends worries you?",
            }
        },
    };

    readonly KeywordReply NoMatchKeywordReply = new KeywordReply() {
        Keywords = { "nokeyfound" },
        Replies = {
            "I'm not sure I understand you fully",
            "I see",
            "What does that suggest to you?",
            "Can you elaborate on that?",
            "That is quite interesting",
            "Please go on",
            "Tell me more about that",
            "Do you feel strongly about discussing such things?",
            "Come, come. Elucidate your thoughts",
            "Please continue",
            "Does talking about this bother you?"
        }
    };

    string NormalizeInput(string userInput) {
        userInput = userInput.ToLower()
            .Replace("\'", "");
        if (userInput.EndsWith(" ") || userInput.EndsWith(".") || userInput.EndsWith("!") || userInput.EndsWith("?")) {
            userInput = userInput.Substring(0, userInput.Length - 1);
        }
        userInput = String.Format(" {0} ", userInput);
        return userInput;
    }

    Tuple<KeywordReply, string> FindKeyword(string input) {
        foreach (KeywordReply keywordReply in keywordreplies) {
            foreach (string keyword in keywordReply.Keywords) {
                string paddedKeyword = String.Format(" {0} ", keyword);
                int index = input.IndexOf(paddedKeyword);
                if (index >= 0) {
                    Debug.Log("keyword found: " + keyword);
                    return Tuple.Create(keywordReply, input.Substring(index + keyword.Length + 1));
                }
            }
        }
        return Tuple.Create(NoMatchKeywordReply, string.Empty);
    }

}
