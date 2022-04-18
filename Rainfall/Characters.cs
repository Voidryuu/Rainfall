using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rainfall
{
    class Characters
    {
        private static Random random = new Random();
        private static readonly string allChars = "abcdefghijklmnopqrstuvwxyz";
        private static readonly string allCharsLarge = @"    _         
U  /""\  u     
 \/ _ \/      
 / ___ \      
/_/   \_\     
 \\    >>     
(__)  (__)    

   ____       
U | __"")u     
 \|  _ \/     
  | |_) |     
  |____/      
 _|| \\_      
(__) (__)     

   ____       
U /""___|      
\| | u        
 | |/__       
  \____|      
 _// \\       
(__)(__)      

  ____        
 |  _""\       
/| | | |      
U| |_| |\     
 |____/ u     
  |||_        
 (__)_)       

U _____ u     
\| ___""|/     
 |  _|""       
 | |___       
 |_____|      
 <<   >>      
(__) (__)     

  _____       
 |"" ___|      
U| |_  u      
\|  _|/       
 |_|          
 )(\\,-       
(__)(_/       

   ____       
U /""___|u     
\| |  _ /     
 | |_| |      
  \____|      
  _)(|_       
 (__)__)      

  _   _       
 |'| |'|      
/| |_| |\     
U|  _  |u     
 |_| |_|      
 //   \\      
(_"") (""_)     


     ___      
    |_""_|     
     | |      
   U/| |\u    
.-,_|___|_,-. 
 \_)-' '-(_/  

     _        
  U |""| u     
 _ \| |/      
| |_| |_,-.   
 \___/-(_/    
  _//         
 (__)         

   _  __      
  |""|/ /      
  | ' /       
U/| . \\u     
  |_|\_\      
,-,>> \\,-.   
 \.)   (_/    

   _          
  |""|         
U | | u       
 \| |/__      
  |_____|     
  //  \\      
 (_"")(""_)     

  __  __      
U|' \/ '|u    
\| |\/| |/    
 | |  | |     
 |_|  |_|     
<<,-,,-.      
 (./  \.)     

  _   _       
 | \ |""|      
<|  \| |>     
U| |\  |u     
 |_| \_|      
 ||   \\,-.   
 (_"")  (_/    

   U  ___ u   
    \/""_ \/   
    | | | |   
.-,_| |_| |   
 \_)-\___/    
      \\      
     (__)     

  ____        
U|  _""\ u     
\| |_) |/     
 |  __/       
 |_|          
 ||>>_        
(__)__)       

   ___        
  / "" \       
 | |""| |      
/| |_| |\     
U \__\_\u     
   \\//       
  (_(__)      

   ____       
U |  _""\ u    
 \| |_) |/    
  |  _ <      
  |_| \_\     
  //   \\_    
 (__)  (__)   

  ____        
 / __""| u     
<\___ \/      
 u___) |      
 |____/>>     
  )(  (__)    
 (__)         

  _____       
 |_ "" _|      
   | |        
  /| |\       
 u |_|U       
 _// \\_      
(__) (__)     

   _   _      
U |""|u| |     
 \| |\| |     
  | |_| |     
 <<\___/      
(__) )(       
    (__)      

 __     __    
 \ \   /""/u   
  \ \ / //    
  /\ V /_,-.  
 U  \_/-(_/   
   //         
  (__)        


 __        __ 
 \""\      /""/ 
 /\ \ /\ / /\ 
U  \ V  V /  U
.-,_\ /\ /_,-.
 \_)-'  '-(_/ 

  __  __      
  \ \/""/      
  /\  /\      
 U /  \ u     
  /_/\_\      
,-,>> \\_     
 \_)  (__)    

  __   __     
  \ \ / /     
   \ V /      
  U_|""|_u     
    |_|       
.-,//|(_      
 \_) (__)     

  _____       
 |""_  /u      
 U / //       
 \/ /_        
 /____|       
 _//<<,-      
(__) (_/      ";
        private static Dictionary<char, LargeText> RainLargeChars = new Dictionary<char, LargeText>();
        private static Dictionary<string, LargeText> RainLargeText = new Dictionary<string, LargeText>();

        static Characters()
        {
            string largeCharSeparator = Environment.NewLine + Environment.NewLine;
            string[] charsLarge = allCharsLarge.Split(largeCharSeparator);
            for (int i = 0; i < allChars.Length; i++)
            {
                string[] charlines = charsLarge[i].Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                RainLargeChars.Add(allChars[i], new LargeText(charlines, charlines.Length, charlines[0].Length));
            }

            RainLargeText.Add("hi", GetLargeText("hi"));
            RainLargeText.Add("cake", GetLargeText("cake"));
            RainLargeText.Add("glad", GetLargeText("glad"));
            RainLargeText.Add("pig", GetLargeText("pig"));
            RainLargeText.Add("lie", GetLargeText("lie"));
            RainLargeText.Add("kill", GetLargeText("kill"));
            RainLargeText.Add("test", GetLargeText("test"));
            RainLargeText.Add("good", GetLargeText("good"));
            RainLargeText.Add("narc", GetLargeText("narc"));
            RainLargeText.Add("dog", GetLargeText("dog"));
            RainLargeText.Add("luck", GetLargeText("luck"));
        }

        public static string[] GetLargeCharEmptyLines(LargeText largeCharacter)
        {
            string emptyLine = "";
            for (int i = 0; i < largeCharacter.Width; i++)
            {
                emptyLine += " ";
            }
            emptyLine += Environment.NewLine;
            string[] emptyLines = new string[largeCharacter.Height];
            for (int i = 0; i < largeCharacter.Height; i++)
            {
                emptyLines[i] = emptyLine;
            }
            return emptyLines;
        }
        public static char GetRandomChar()
        {
            int index = random.Next(RainLargeChars.Count);
            return RainLargeChars.ElementAt(index).Key;
        }
        public static string GetRandomText()
        {
            int index = random.Next(RainLargeText.Count);
            return RainLargeText.ElementAt(index).Key;
        }

        public static LargeText GetLargeText(string text)
        {
            List<LargeText> largeCharacters = new List<LargeText>();
            int heightLargestCharacter = 0;
            for (int charIndex = 0; charIndex < text.Length; charIndex++)
            {
                LargeText largeCharacter = RainLargeChars[text[charIndex]];
                largeCharacters.Add(largeCharacter);
                if (largeCharacter.Height > heightLargestCharacter) { heightLargestCharacter = largeCharacter.Height; }
            }
            string[] largeTextLines = new string[heightLargestCharacter];
            for (int charIndex = 0; charIndex < text.Length; charIndex++)
            {
                int largeCharHeight = largeCharacters[charIndex].Height;
                for (int lineIndex = 0; lineIndex < largeCharHeight; lineIndex++)
                {
                    string charLine = largeCharacters[charIndex].Text[lineIndex];
                    largeTextLines[lineIndex] += charLine;
                }
            }
            LargeText largeText = new LargeText(largeTextLines, largeTextLines.Length, largeTextLines[0].Length);
            return largeText;
        }
    }
}
