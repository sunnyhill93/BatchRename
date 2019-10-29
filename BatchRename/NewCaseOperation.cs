﻿using System;
using System.Text;

namespace BatchRename
{
    public class CaseArg : StringArgs
    {
        public string Case { get; set; }
    }

    public class NewCaseStringOperation : StringOperation
    {
        public override string Name => "Change Case";

        public override string Description
        {
            get
            {
                var arg = Args as CaseArg;
                return ($"Change the case format of the string to {arg.Case}");
            }
        }

        public NewCaseStringOperation()
        {
            Args = new CaseArg()
            {
                Case = "Lower",
            };
        }

        static string ToUpperFirstLetter(string input)
        {
            StringBuilder result = new StringBuilder(input);

            if (result[0] >= 'a' && result[0] <= 'z')
            {
                result[0] = (char)('A' + (input[0] - 'a'));
            }

            for (int i = 1; i < input.Length; i++)
            {
                if (input[i] >= 'a' && input[i] <= 'z' && input[i - 1] == ' ')
                {
                    result[i] = (char)('A' + (input[i] - 'a')); //change le
                }
            }
            return result.ToString();
        }

        public override void OpenDialog()
        {
            var screen = new ChangeCaseDialog(Args);
            screen.OptArgsChange += ChangeCaseArg;
            if (screen.ShowDialog() == true)
            {
                Notify("Description");
            }
        }

        public override string OperateString(string input)
        {
            string result = input;
            var arg = Args as CaseArg;

            if (arg.Case == "Lower")
            {
                result = input.ToLower();
            }
            if (arg.Case == "Upper")
            {
                result = input.ToUpper();
            }
            if (arg.Case == "Upper First Letter")
            {
                result = ToUpperFirstLetter(input);
            }
            return result;
        }

        void ChangeCaseArg(string ChosenCase)
        {
            (Args as CaseArg).Case = ChosenCase;
        }

        public override StringOperation Clone()
        {
            var oldArgs = Args as CaseArg;
            return new NewCaseStringOperation()
            {
                Args = new CaseArg()
                {
                    Case = oldArgs.Case
                }
            };
        }
    }
}
