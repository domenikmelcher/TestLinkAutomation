using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Plossum.CommandLine;

namespace TestLinkAutomation.SVN.LogAnalyzer.CommandLine
{
    [CommandLineManager()]
    public class Options
    {
        [CommandLineOption(Description = "Displays this help text")]
        public bool Help = false;

        [CommandLineOption(Description = "Specifies the input file in unified diff format", MinOccurs = 1)]
        public string input
        {
            get { return input_string; }
            set
            {
                if (String.IsNullOrEmpty(value))
                    throw new InvalidOptionValueException(
                        "The input diff file must not be empty", false);
                input_string = value;
            }
        }
        private string input_string;

        [CommandLineOption(Description = "Specifies the output file path")]
        public string output
        {
            get { return output_string; }
            set
            {
                output_string = value;
            }
        }

        private string output_string;

        [CommandLineOption(Description = "Specifies the SVN repository", MinOccurs = 1)]
        public string svn_repository
        {
            get { return svn_repo_string; }
            set
            {
                svn_repo_string = value;
            }
        }

        private string svn_repo_string;

        [CommandLineOption(Description = "Specifies the encrypted user name of the SVN account", MinOccurs = 1)]
        public string svn_user
        {
            get { return svn_user_string; }
            set
            {
                svn_user_string = value;
            }
        }

        private string svn_user_string;

        [CommandLineOption(Description = "Specifies the encrypted password of the SVN account", MinOccurs = 1)]
        public string svn_password
        {
            get { return svn_password_string; }
            set
            {
                svn_password_string = value;
            }
        }

        private string svn_password_string;

        [CommandLineOption(Description = "Specifies the revision that should be used for SVN diff", MinOccurs = 1)]
        public int svn_revision
        {
            get { return svn_revision_string; }
            set
            {
                svn_revision_string = value;
            }
        }

        private int svn_revision_string;
    }
}
