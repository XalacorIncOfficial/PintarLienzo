using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Ralf Stefan Schiegerl - Pintar Lienzo


public class AppManager : MonoBehaviour
{
    // Erkennung vom Board
    public Renderer Board;
    public Renderer CurrentColor;
    private Texture2D texture;

    // Erkennung des Clicks aufs Chroma
    private Ray RayInfo;
    private RaycastHit HitInfo;

    private Color MyColor;

    public Texture2D CurrentBrush;

    // Radiergummi
    private Color EraseColor = new Color(1, 1, 1, 1);
    private bool ActiveErase;

    // Eyedroppertool
    private bool _cuentaGotas;

 


    void Start()
    {
        texture = new Texture2D(2000, 2000);                                                // Texturgröße 
        Board.material.mainTexture = texture;
        CurrentColor.material.color = new Color(0, 0, 0, 1);
        MyColor = CurrentColor.material.color;
    }


    void Update()
    {
        RayInfo = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(RayInfo, out HitInfo))
        {

            // Chroma
            if (Input.GetMouseButtonDown(0))                                                     // 0 := linke Maustaste
            {
                if (HitInfo.collider.tag == "Chroma")
                {
                    Texture2D ChromaTexture = (Texture2D)HitInfo.collider.GetComponent<Renderer>().material.mainTexture;            // Konvertierung einer Textur in eine 2D Textur

                    Vector2 TempCoordinates = HitInfo.textureCoord;
                    TempCoordinates.x *= ChromaTexture.width;
                    TempCoordinates.y *= ChromaTexture.height;

                    Color FinalColor = ChromaTexture.GetPixel((int)TempCoordinates.x, (int)TempCoordinates.y);


                    if (FinalColor.a != 0)
                    {
                        CurrentColor.material.color = FinalColor;

                        MyColor = FinalColor;
                    }
                }
            }

            // Board
            if (Input.GetMouseButton(0))
            {
                if (HitInfo.collider.tag == "Board")
                {
                    if (_cuentaGotas == true)
                    {
                        Texture2D BoardTexture = (Texture2D)HitInfo.collider.GetComponent<Renderer>().material.mainTexture;

                        Vector2 BoardCord = HitInfo.textureCoord;

                        BoardCord.x *= BoardTexture.width;
                        BoardCord.y *= BoardTexture.height;

                        Color BoardColor = BoardTexture.GetPixel((int)BoardCord.x, (int)BoardCord.y);

                        CurrentColor.material.color = BoardColor;
                        MyColor = BoardColor;
                    }
                    else
                    {
                        Vector2 TempCoordinates = HitInfo.textureCoord;
                        TempCoordinates.x *= texture.width;
                        TempCoordinates.y *= texture.height;

                        for (int i = 0; i < CurrentBrush.width; i++)
                        {
                            for (int j = 0; j < CurrentBrush.height; j++)
                            {
                                // Farbe auslesen
                                Color TempColor = CurrentBrush.GetPixel(i, j);

                                // Pinsel zentrieren
                                if (TempColor.a != 0)
                                {
                                    // Breite (i) und Höhe (j) + Zentriert 
                                    Vector2 NewCoordinate = TempCoordinates;
                                    NewCoordinate.x += i - (CurrentBrush.width / 2);
                                    NewCoordinate.y += j - (CurrentBrush.height / 2);

                                    if (ActiveErase == true)
                                    {
                                        texture.SetPixel((int)NewCoordinate.x, (int)NewCoordinate.y, EraseColor);
                                    }
                                    else
                                    {
                                        texture.SetPixel((int)NewCoordinate.x, (int)NewCoordinate.y, MyColor);
                                    }
                                }
                            }
                        }
                        texture.Apply();
                    }
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            GameObject newBoard = new GameObject("PhysicsBoard");
            Vector3 finalPos = Board.transform.position;                                                                        // Position
            finalPos.z = -1;
            newBoard.transform.position = finalPos;
            newBoard.AddComponent<SpriteRenderer>();                                                                            // Hinzufügen SpriteRenderer
            float finalScale = Board.transform.localScale.x / 20;                                                               // Re-Scale des PhysicsBoards
            newBoard.transform.localScale = new Vector3(finalScale, finalScale, finalScale);

            Texture2D alphaTexture = CreateAlpha(texture);

            Sprite newSprite = Sprite.Create(alphaTexture, new Rect(0, 0, alphaTexture.width, alphaTexture.height), new Vector2(0.5f, 0.5f));  // 
            newBoard.GetComponent<SpriteRenderer>().sprite = newSprite;

            newBoard.AddComponent<PolygonCollider2D>();
            newBoard.AddComponent<Rigidbody2D>();


            // Clearing Board
            texture = new Texture2D(2000, 2000);                                                // Texturgröße 
            Board.material.mainTexture = texture;
            
        }

    }

    private Texture2D CreateAlpha(Texture2D _texture)
    {
        Texture2D newAlphaTexture = new Texture2D(_texture.width, _texture.height);                                             // Erzeugung einer neuen Textur für die Zeichnung

        Color[] tempColor = _texture.GetPixels();                                                                               // Array of Colors
        for (int i = 0; i < tempColor.Length; i++)
        {
            if (tempColor[i].a == 0)                                                                                            // a = Alpha
            {
                tempColor[i].a = 0;
            }
        }
        newAlphaTexture.SetPixels(tempColor);
        newAlphaTexture.Apply();
        return newAlphaTexture;
    }


    // Pinselwechsel
    public void ChangeBrush(Texture2D brush)
    {
        CurrentBrush = brush;
    }

    // Löschen/Radiergummi
    public void SetActiveErase(Image BtnColor)
    {
        ActiveErase = !ActiveErase;
        _cuentaGotas = false;

        if (ActiveErase == true)
        {
            BtnColor.color = Color.grey;
        }
        else
        {
            BtnColor.color = Color.white;
        }
    }

    // Screenshot
    public void SetCapture()
    {
        string fileName = "PintarLienzo";
        string Year = System.DateTime.Now.ToString("_yyyy-MM-dd");
        string Hour = System.DateTime.Now.ToString(":HH-mm-ss");
        string Format = ".png";

        ScreenCapture.CaptureScreenshot(fileName + Year + Hour + Format);
    }

    // Pipette
    public void CuentaGotas(Image BtnColor)
    {
        _cuentaGotas = !_cuentaGotas;
        ActiveErase = false;

        if (_cuentaGotas == true)
        {
            BtnColor.color = Color.grey;
        }
        else
        {
            BtnColor.color = Color.white;
        }
    }

}
