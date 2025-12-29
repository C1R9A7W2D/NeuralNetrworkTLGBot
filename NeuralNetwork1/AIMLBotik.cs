using AIMLbot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetwork1
{
    class AIMLBotik
    {
        Bot myBot;
        User myUser;  ///   map[TLGUserID] -> AIML User ID

        public AIMLBotik()
        {
            myBot = new Bot();
            myBot.loadSettings();
            myUser = new User("TLGUser", myBot);
            myBot.isAcceptingUserInput = false;
            myBot.loadAIMLFromFiles();
            myBot.isAcceptingUserInput = true;
        }

        public string Talk(string phrase)
        {
            Request r = new Request(phrase, myUser, myBot);
            Result res = myBot.Chat(r);
            return res.Output;
        }

        public void SetUserName(string name)
        {
            SetAIMLVariable("username", name);
        }

        /// <summary>
        /// Универсальная обёртка для установки любой переменной
        /// </summary>
        /// <param name="varName"></param>
        /// <param name="value"></param>
        void SetAIMLVariable(string varName, object value)
        {
            string strValue = Convert.ToString(value);
            if (myUser.Predicates.containsSettingCalled(varName))
                myUser.Predicates.updateSetting(varName, strValue);
            else
                myUser.Predicates.addSetting(varName, strValue);
        }
    }
}
