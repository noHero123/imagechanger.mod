using System;

using ScrollsModLoader.Interfaces;
using UnityEngine;
using Mono.Cecil;
//using Mono.Cecil;
//using ScrollsModLoader.Interfaces;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Reflection;
using JsonFx.Json;
using System.Text.RegularExpressions;


namespace imagechanger.mod
{
    public class imagechanger : BaseMod, ICommListener
	{
        private string ownpicpath = Environment.CurrentDirectory + System.IO.Path.DirectorySeparatorChar + "ownimages" + System.IO.Path.DirectorySeparatorChar;
        private int[] cardids;
        private string[] cardnames;
        private int[] cardImageid;
        private string[] imagefiles;
		//initialize everything here, Game is loaded at this point
        public imagechanger()
		{
            try
            {
                App.Communicator.addListener(this);
            }
            catch { }
            this.imagefiles=Directory.GetFiles(this.ownpicpath, "*.png");
            
		}


		public static string GetName ()
		{
			return "Imagechanger";
		}

		public static int GetVersion ()
		{
			return 1;
		}

        private int cardimageidtoid(int imageid) { return cardids[Array.FindIndex(cardImageid, element => element.Equals(imageid))]; }
        private string cardimageidtoname(int imageid) { return cardnames[Array.FindIndex(cardImageid, element => element.Equals(imageid))]; }
        private int cardidtoimageid(int id) { return cardImageid[Array.FindIndex(cardids, element => element.Equals(id))]; }
        private string cardidtoname(int id) { return cardnames[Array.FindIndex(cardids, element => element.Equals(id))]; }
        private int cardnametoid(string name) { return cardids[Array.FindIndex(cardnames, element => element.Equals(name))]; }
        private int cardnametoimageid(string name) { return cardImageid[Array.FindIndex(cardnames, element => element.Equals(name))]; }


        public void handleMessage(Message msg)
        {

            if (msg is CardTypesMessage)
            {

                JsonReader jsonReader = new JsonReader();
                Dictionary<string, object> dictionary = (Dictionary<string, object>)jsonReader.Read(msg.getRawText());
                Dictionary<string, object>[] d = (Dictionary<string, object>[])dictionary["cardTypes"];
                this.cardids = new int[d.GetLength(0)];
                this.cardnames = new string[d.GetLength(0)];
                this.cardImageid = new int[d.GetLength(0)];

                for (int i = 0; i < d.GetLength(0); i++)
                {
                    cardids[i] = Convert.ToInt32(d[i]["id"]);
                    cardnames[i] = d[i]["name"].ToString();
                    cardImageid[i] = Convert.ToInt32(d[i]["cardImage"]);
                }
                
                App.Communicator.removeListener(this);//dont need the listener anymore
            }
            
            return;
        }
        public void onReconnect()
        {
            return; // don't care
        }

       

		//only return MethodDefinitions you obtained through the scrollsTypes object
		//safety first! surround with try/catch and return an empty array in case it fails
		public static MethodDefinition[] GetHooks (TypeDefinitionCollection scrollsTypes, int version)
		{
            try
            {
                return new MethodDefinition[] {
                    scrollsTypes["AssetLoader"].Methods.GetMethod("LoadTexture2D", new Type[]{typeof(string)}),
                    scrollsTypes["CardView"].Methods.GetMethod("applyCardTexture")[0],

             };
            }
            catch
            {
                return new MethodDefinition[] { };
            }
		}


        public override bool BeforeInvoke(InvocationInfo info, out object returnValue)
        {
           
            if (info.targetMethod.Equals("LoadTexture2D"))
            {
                string imagenumber = (string)info.arguments[0];
                if (imagenumber.Equals("0") == false)
                {
                    string name = cardimageidtoname(Convert.ToInt32(imagenumber));
                    
                    if (imagefiles.Contains(this.ownpicpath + name + "_prev.png"))//File.Exists() was slower
                    {

                        Texture2D texture2D = new Texture2D(2, 2, TextureFormat.RGB24, false);
                        byte[] data = File.ReadAllBytes(this.ownpicpath + name + "_prev.png");
                        texture2D.LoadImage(data);
                        returnValue = texture2D;
                        return true;
                    }
                    else
                    {
                        if (imagefiles.Contains(this.ownpicpath + name + ".png"))//File.Exists() was slower
                        {

                            Texture2D texture2D = new Texture2D(2, 2, TextureFormat.RGB24, false);
                            byte[] data = File.ReadAllBytes(this.ownpicpath + name + ".png");
                            texture2D.LoadImage(data);
                            returnValue = texture2D;
                            return true;
                        }
                    }
                }
            }


            if (info.targetMethod.Equals("applyCardTexture"))
            {
                
                Card card = (Card)typeof(CardView).GetField("cardInfo", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(info.target);

                if (imagefiles.Contains(this.ownpicpath + card.getName() + ".png"))//(File.Exists(this.ownpicpath + card.getName() + ".png"))
                {
                    GameObject cardImage = (GameObject)typeof(CardView).GetField("cardImage", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(info.target);
                    iCardRule callBackTarget = (iCardRule)typeof(CardView).GetField("callBackTarget", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(info.target);
                    Texture2D texture2D = new Texture2D(2, 2, TextureFormat.RGB24, false);
                    byte[] data = File.ReadAllBytes(this.ownpicpath + card.getName()+".png");
                    texture2D.LoadImage(data);
                    cardImage.renderer.material.color = Color.white;
                    cardImage.renderer.material.mainTexture = texture2D;
                    cardImage.renderer.enabled = true;
                    if (callBackTarget != null)
                    {
                        callBackTarget.SetLoadedImage(texture2D, card.getCardImage().ToString());
                    }
                    else
                    {
                        //typeof(CardView).GetMethod("SetLoadedImage", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(info.target, new object[] { texture2D, card.getCardImage().ToString() });
                        //method is static so dont info.target must be null and search statics, too (bindflags.static)
                        typeof(CardView).GetMethod("SetLoadedImage", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).Invoke(null, new object[] { texture2D, card.getCardImage().ToString() });
                    }
                    Boolean hasSetHighResTexture = (Boolean)typeof(CardView).GetField("hasSetHighResTexture", BindingFlags.Instance | BindingFlags.NonPublic ).GetValue(info.target);
                    hasSetHighResTexture = true;
                    returnValue = null;
                    return true;

                }


            }
         

            returnValue = null;
            return false;
        }

		public override void AfterInvoke (InvocationInfo info, ref object returnValue)
		{

           



			return;
		}



        
	}
}

