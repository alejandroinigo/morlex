using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeywordReply
{
    public List<string> Keywords = new List<string>();
    public List<string> Replies = new List<string>();

    int currentReply = 0;

    static readonly List<Tuple<string, string>> conjugate = new List<Tuple<string, string>> {
        Tuple.Create(" are "    , " am "    ),
        Tuple.Create(" were "   , " was "   ),
        Tuple.Create(" you "    , " i "     ),
        Tuple.Create(" your "   , " my "    ),
        Tuple.Create(" ive "    , " youve " ),
        Tuple.Create(" im "     , " youre " ),
        Tuple.Create(" me "     , " you " ),
        Tuple.Create(" yourself "     , " myself " ),
    };

    string ReverseConjugation(string input) {
        foreach(Tuple<string, string> pair in conjugate) {
            if (input.Contains(pair.Item1)) {
                input = input.Replace(pair.Item1, pair.Item2);
            } else if (input.Contains(pair.Item2)) {
                input = input.Replace(pair.Item2, pair.Item1);
            }
        }
        return input;
    }

    public string FormatReply(string suffix) {
        string reply = Replies[currentReply];
        var index = reply.IndexOf("*");
        if (index >= 0) {
            if (suffix.EndsWith(" ")) {
                suffix = suffix.Substring(0, suffix.Length - 1);
            }
            reply = reply.Replace("*", (suffix == "" ? "" : ReverseConjugation(suffix)));
        }
        currentReply = (currentReply + 1) % Replies.Count;
        return reply;
    }
}
