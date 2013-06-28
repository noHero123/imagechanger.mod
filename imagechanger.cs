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
        private string[] backgroundimages;
		//initialize everything here, Game is loaded at this point
        public imagechanger()
		{
            try
            {
                App.Communicator.addListener(this);
            }
            catch { }
            this.imagefiles=Directory.GetFiles(this.ownpicpath, "*.png");
            this.backgroundimages=Directory.GetFiles(this.ownpicpath, "background_*.png");
            
		}

        private string rndbackground() {
            if (backgroundimages.Length > 0)
            {
                System.Random random = new System.Random();
                int randomNumber = random.Next(0, backgroundimages.Length);

                return backgroundimages[randomNumber];
            }
            else { return ""; }
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
                    scrollsTypes["BattleMode"].Methods.GetMethod("setupBackground", new Type[]{typeof(int)}),
                    scrollsTypes["CardView"].Methods.GetMethod("applyCardTexture")[0],

             };
            }
            catch
            {
                return new MethodDefinition[] { };
            }
		}

        public override bool BeforeInvoke(InvocationInfo info, out object returnValue) {
            if (info.targetMethod.Equals("setupBackground"))
            {
                int backGround = (int)info.arguments[0];
                BackgroundData background = BackgroundData.getBackground(backGround);
                BackgroundData currentBgData = (BackgroundData)typeof(BattleMode).GetField("currentBgData", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(info.target);
                    
                currentBgData = background;
                Unit.shadowColor = background.shadowColor;
                int num = -1;
                while (true)
                {
                    Material material = new Material(Shader.Find("Transparent/Diffuse"));
                    material.mainTextureOffset = new Vector2(0.01f, 0.01f);
                    material.mainTextureScale = new Vector2(0.98f, 0.98f);
                    WorldImage image = background.getImage(++num);
                    if (image == null)
                    {
                        break;
                    }
                    GameObject gameObject = PrimitiveFactory.createPlane(false);
                    string path = rndbackground();
                    Texture2D mainTexture;
                    if (path.Equals("")) {
                         mainTexture = ResourceManager.LoadTexture(image.filename);


                         GameObject lightSource = (GameObject)typeof(BattleMode).GetField("lightSource", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(info.target);
                         GameObject lightSource2 = (GameObject)typeof(BattleMode).GetField("lightSource2", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(info.target);

                         if (backGround == 1)
                         {
                             BattleMode lol = (BattleMode)info.target;
                             GameObject gameObject2 = new GameObject();
                             gameObject2.AddComponent<MeshRenderer>();
                             gameObject2.AddComponent<EffectPlayer>();
                             EffectPlayer component = gameObject2.GetComponent<EffectPlayer>();
                             component.init("torch1", 1, lol, 50, new Vector3(0.5f, 0.5f, 0.5f), true, string.Empty, 0);
                             gameObject2.transform.position = new Vector3(-3.4f, 0.75f, 5.565f);
                             gameObject2.transform.eulerAngles = new Vector3(51f, 270f, 0f);
                             GameObject gameObject3 = new GameObject();
                             gameObject3.AddComponent<MeshRenderer>();
                             gameObject3.AddComponent<EffectPlayer>();
                             EffectPlayer component2 = gameObject3.GetComponent<EffectPlayer>();
                             component2.init("torch1", 1, lol, 50, new Vector3(0.5f, 0.5f, 0.5f), true, string.Empty, 0);
                             gameObject3.transform.position = new Vector3(-3.4f, 0.75f, -5.7225f);
                             gameObject3.transform.eulerAngles = new Vector3(51f, 270f, 0f);

                             lightSource = new GameObject("Light");
                             lightSource.AddComponent<Light>();
                             lightSource.light.color = Color.yellow;
                             lightSource.light.cullingMask = ~(1 << BattleMode.LAYER_NOLIGHT);
                             lightSource.transform.position = new Vector3(-2.86f, 0.75f, 5.37f);
                             lightSource.light.intensity = 0.44f;
                             lightSource.light.type = LightType.Point;
                             lightSource.light.range = 10f;
                             lightSource.light.shadows = LightShadows.Soft;
                             lightSource2 = new GameObject("Light");
                             lightSource2.AddComponent<Light>();
                             lightSource2.light.color = Color.yellow;
                             lightSource2.light.cullingMask = ~(1 << BattleMode.LAYER_NOLIGHT);
                             lightSource2.transform.position = new Vector3(-2.86f, 0.75f, -5.48f);
                             lightSource2.light.intensity = 0.44f;
                             lightSource2.light.type = LightType.Point;
                             lightSource2.light.range = 10f;
                             lightSource2.light.shadows = LightShadows.Soft;
                         }
                         if (backGround == 4)
                         {
                             Color color = new Color(0.972549f, 0.5568628f, 0.219607845f);
                             lightSource = new GameObject("Light");
                             lightSource.AddComponent<Light>();
                             lightSource.light.color = Color.yellow;
                             lightSource.light.cullingMask = ~(1 << BattleMode.LAYER_NOLIGHT);
                             lightSource.transform.position = new Vector3(-2.93f, 0.75f, 4.08f);
                             lightSource.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                             lightSource.light.intensity = 0.44f;
                             lightSource.light.type = LightType.Point;
                             lightSource.light.range = 3f;
                             lightSource.light.shadows = LightShadows.Soft;
                             lightSource2 = new GameObject("Light");
                             lightSource2.AddComponent<Light>();
                             lightSource2.light.color = Color.yellow;
                             lightSource2.light.cullingMask = ~(1 << BattleMode.LAYER_NOLIGHT);
                             lightSource2.transform.position = new Vector3(-3.46f, 0.75f, -3.66f);
                             lightSource2.light.intensity = 0.44f;
                             lightSource2.light.type = LightType.Point;
                             lightSource2.light.range = 3f;
                             lightSource2.light.shadows = LightShadows.Soft;
                         }
                    }
                    else
                    {
                         mainTexture = new Texture2D(2, 2, TextureFormat.RGB24, false);
                        byte[] data = File.ReadAllBytes(path);
                        mainTexture.LoadImage(data);
                    }
                    //
                    
                    material.renderQueue = 2 + num;
                    gameObject.renderer.material = material;
                    gameObject.renderer.material.mainTexture = mainTexture;
                    gameObject.transform.position = image.pos;
                    gameObject.transform.localScale = image.scale;
                    gameObject.transform.eulerAngles = image.rot;
                }
                
                Camera.main.transform.eulerAngles = new Vector3(51f, 270f, 0f);
                Camera.main.transform.position = new Vector3(9.67f, 11.15f, 0f);


                returnValue = null;
                return true;
            }

            returnValue = null;
            return false;
        
        }

        public override void AfterInvoke (InvocationInfo info, ref object returnValue)
        //public override bool BeforeInvoke(InvocationInfo info, out object returnValue)
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
                        return;//return true;
                    }
                    else
                    {
                        if (imagefiles.Contains(this.ownpicpath + name + ".png"))//File.Exists() was slower
                        {

                            Texture2D texture2D = new Texture2D(2, 2, TextureFormat.RGB24, false);
                            byte[] data = File.ReadAllBytes(this.ownpicpath + name + ".png");
                            texture2D.LoadImage(data);
                            returnValue = texture2D;
                            return;// return true;
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
                    //returnValue = null;
                    return;//return true;

                }


            }
         

            //returnValue = null;
            return;//return false;
        }

		/*public override void AfterInvoke (InvocationInfo info, ref object returnValue)
		{

           



			return;
		}*/



        
	}
}

