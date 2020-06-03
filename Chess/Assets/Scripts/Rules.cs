using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChessLogics;
using System;
using UnityEngine.UI;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using System.Transactions;

//using UnityEngine.UIElements;

public class Rules : MonoBehaviour
{
    // Start is called before the first frame update

    public static Rules Instance { set; get; }

    DragAndDrop dad;

    public Chess temp;
    public Chess chess { set; get; }
    public UnityEngine.UI.Button Queen, Bishop, Knight, Rook, Exit, GameExit;
    public Text WinLoseText, textExit, textExitGame, Players;

    public bool isWhite;

    private Client client;

    public static string FEN { set; get; }

    public static string MOVE { set; get; }

    public static int PROMOTION { set; get; }

    int k = 0;

   // public bool Lose { set; get; }

    public Rules()
    {
        chess = new Chess();
        temp =  new Chess();
        dad = new DragAndDrop();
    }
    public void Start()
    {
        Instance = this;
        client = FindObjectOfType<Client>();
        isWhite = client.isHost;
        ShowFigures();
        MarkValidFigures();
        
    }

    // Update is called once per frame
     void Update()
     {
       
        if(MOVE != "" && MOVE != null)
        {
            chess = chess.Move(MOVE, PROMOTION, 1);
            MOVE = "";
            ShowFigures();
            MarkValidFigures();
        }
        if (dad.Action())
        {
            string from = GetSquare(dad.pickPosition);//Pe2e4
            string to= GetSquare(dad.dropPosition);
            string figure = chess.GetFigureAt((int)(dad.pickPosition.x / 2.0), (int)(dad.pickPosition.y / 2.0)).ToString();
            string move = Convert.ToString(figure + from + to);
            //if (!(chess.board.moveNumber % 2 == 0 && isWhite ==false))
            //    goto NotYourTurn;
            StartCoroutine(MyCoroutine(move, figure));
            ShowFigures();
            
        }
        if (chess.isCheckmate())
        {
            Exit.interactable = true;
            GameExit.interactable = true;
            textExitGame.text = "Quit the game";
            textExit.text = "Exit to menu";
            if (chess.board.moveColor == ChessLogics.Color.white)
                WinLoseText.text = "White lose...";
            else
                WinLoseText.text = "Black lose...";
            //client.Send("LOSE|");
            Exit.onClick.AddListener(ExitToMenu);
            GameExit.onClick.AddListener(QuitTheGame);
        }
        if (chess.isStalemate())
        {
            Exit.interactable = true;
            GameExit.interactable = true;
            textExitGame.text = "Quit the game";
            textExit.text = "Exit to menu";
            WinLoseText.text = "Stalemate";
            //client.Send("LOSE|");
            Exit.onClick.AddListener(ExitToMenu);
            GameExit.onClick.AddListener(QuitTheGame);
        }
    }

    void QuitTheGame()
    {
        Application.Quit();    }
    void ExitToMenu()
    {
        SceneManager.LoadScene("Menu");
        
    }
    
    
    IEnumerator MyCoroutine(string move, string figure)
    {
        
        float my_delay = 2.0f;
        temp = chess;
        temp.Move(move, 0, 0);
        if (temp.FlagPromotion && temp.Correct)
        {
            btnOn();
            if (Char.IsLower(figure[0]))
            {
                Queen.GetComponent<Image>().sprite = Resources.Load<Sprite>("BlackQueen");
                Bishop.GetComponent<Image>().sprite = Resources.Load<Sprite>("BlackBishop");
                Rook.GetComponent<Image>().sprite = Resources.Load<Sprite>("BlackRook");
                Knight.GetComponent<Image>().sprite = Resources.Load<Sprite>("BlackKnight");
            }
            else
            {
                Queen.GetComponent<Image>().sprite = Resources.Load<Sprite>("WhiteQueen");
                Bishop.GetComponent<Image>().sprite = Resources.Load<Sprite>("WhiteBishop");
                Rook.GetComponent<Image>().sprite = Resources.Load<Sprite>("WhiteRook");
                Knight.GetComponent<Image>().sprite = Resources.Load<Sprite>("WhiteKnight");
            }
            Queen.onClick.AddListener(delegate() { ChooseQueen(ref my_delay); });
           
            Bishop.onClick.AddListener(delegate() { ChooseBishop(ref my_delay); });
            Rook.onClick.AddListener(delegate() { ChooseRook(ref my_delay); });
            Knight.onClick.AddListener(delegate() { ChooseKnight(ref my_delay); });

            yield return new WaitForSeconds(my_delay);
        }
        if (k == 0)
            k = 1;
        chess = chess.Move(move, k, 1);
        if (temp.Correct)
        {
            client.Send("MOVE|" + move + "|" + k.ToString());
            for (int y = 0; y < 8; y++)
                for (int x = 0; x < 8; x++)
                    MarkSquare(x, y, false);
        }
            
        //client.Send("FEN|"  + chess.fen);
        
    }

    
    void ChooseQueen(ref float delay)
    {
        k = 1;
        btnOff();
        delay = 0;
    }
    void ChooseBishop(ref float delay)
    {
        k = 2;
        btnOff();
        delay = 0;
    }
    void ChooseRook(ref float delay)
    {
        k = 3;
        btnOff();
        delay = 0;
    }
    void ChooseKnight(ref float delay)
    {
        k = 4;
        btnOff();
        delay = 0;
    }
    void btnOff()
    {
        Queen.interactable = false;
        Bishop.interactable = false;
        Rook.interactable = false;
        Knight.interactable = false;
    }
    void btnOn()
    {
        Queen.interactable = true;
        Bishop.interactable = true;
        Rook.interactable = true;
        Knight.interactable = true;
    }



    //Метод преобразует вектор в строку понятную человеку Pe2e4
    public static string GetSquare(Vector2 position)//e2
    {
        int x = Convert.ToInt32(position.x / 2.0);
        int y = Convert.ToInt32(position.y / 2.0);
        return ((char)('a' + x)).ToString() + (y+1).ToString();
    }

    public void GetCoord(string e2, out int x, out int y)
    {
        x = 9;
        y = 9;
        if (e2.Length == 2 &&
            e2[0] >= 'a' &&
            e2[0] <= 'h' &&
            e2[1] >= '1' &&
            e2[1] <= '8')
        {
            x = e2[0] - 'a';
            y = e2[1] - '1';
        }

    }

    //функция для прорисовки всех фигур вначале
    public void ShowFigures()  
    {
        //32 объекта с разными названиями+пешки могут превратиться в другую фигуру+8 объектов каждого типа неудобно
        //лучший способ создать 64 объекта(box) клетки на которых можно нарисать всё что угодно
        int nr = 0;//счётчик оъектов на доске
        for(int y = 0; y < 8; y++)
            for(int x = 0; x < 8; x++)
            {
                string figure = chess.GetFigureAt(x, y).ToString();
                if (figure == ".") continue;
                PlaceFigure("box" + nr,figure,x, y);
                nr++;                            
            }
        //если какую-то фигуру съели надо докрутить счётчик до 32,
        //чтобы все остальные фигуры(незадействованные) перенести на 65 "воображаемую" клетку доски
        for (; nr < 32; nr++)
            PlaceFigure("box" + nr, "q", 9, 9);  
    }

    //метод показывает фигуры,которые могут ходить
    public void MarkValidFigures()
    {
        for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
                MarkSquare(x, y, false);
        foreach(string moves in chess.GetAllMoves())
        {
            //Pe2e4
            // 00   там где нули надо подкрасить
            int x, y;
            GetCoord(moves.Substring(1, 2), out x, out y);
            MarkSquare(x, y, true);
        }

    }

    public void MarkSelectedFigureMoves(string move)
    {
        for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
                MarkSquare(x, y, false);
        foreach (string moves in Rules.Instance.chess.GetAllMovesSelectedFigure(move))
        {
            //Pe2e4
            // 00   там где нули надо подкрасить
            int x, y;
            GetCoord(moves.Substring(3, 2), out x, out y);
            MarkSquare(x, y, true);
        }

    }









    //Метод для пометки квадрата куда мы можем пойти
    public void MarkSquare(int x, int y, bool isMarked)
    {
        GameObject goSquare = GameObject.Find("" + y + x);
        GameObject goCell;
        string color = (x + y) % 2 == 0 ? "Black" : "White";
        if (isMarked)
            goCell = GameObject.Find(color + "SquareMarked");
        else
            goCell = GameObject.Find(color + "Square");
        var spriteSquare = goSquare.GetComponent<SpriteRenderer>();
        var spriteCell = goCell.GetComponent<SpriteRenderer>();

        spriteSquare.sprite = spriteCell.sprite;
    }

    void PlaceFigure(string box,string figure,int x,int y)
    {
        GameObject goBox = GameObject.Find(box);//нашли клетку на которой будем отрисовывать фигуру
        GameObject goFigure = GameObject.Find(figure);//какую фигуру рисуем
        GameObject goSquare = GameObject.Find("" + y + x);//ищем квадрат на котором рифуем данную фигуру

        var spriteFigure = goFigure.GetComponent<SpriteRenderer>();//сохраняем спрайт нужной фигуры в переменную 
        var spritegoBox = goBox.GetComponent<SpriteRenderer>();//сохраняем спрайт нужной клетки в переменную 
        spritegoBox.sprite = spriteFigure.sprite;//рисуем

        goBox.transform.position = goSquare.transform.position;//ставим клетку с фигурой на нужную позицию
    }
}
class DragAndDrop
{
    enum State
    {
        none,
        drag
    }


    //Позиции откуда взяли и куда поставили фигуру
    public Vector2 pickPosition { get; private set; }

    public Vector2 dropPosition { get; private set; }

    State state;//состояние фигуры
    GameObject item;//объект,который перемещаем
    Vector2 offset;//смещение(для захвата фигуры за край)

    public DragAndDrop()
    {
        state = State.none;
        item = null;
    }


    //Обработка событий
    public bool Action()
    {
        switch (state)
        {
            case State.none:
                if (IsMouseButtonPressed())
                    PickUp();
                break;

            case State.drag:
                if (IsMouseButtonPressed())
                    Drag();
                else
                {
                    Drop();
                    return true;
                }
                break;

        }
        return false;
    }

 
    //Проверка на нажатие ЛКМ
    bool IsMouseButtonPressed()
    {
        return Input.GetMouseButton(0);
    }


   

    //Захват фигуры
    void PickUp()
    {
        Vector2 clickPosition = GetClickPosition();
        Transform clickedItem = GetItemAt(clickPosition);
        //если мы не попали ни по какой фигуре
        if (clickedItem == null) return;
        //если мы пытаемся двигать не нашу фигуру
        if ((Rules.Instance.chess.board.moveNumber % 2 == 0 && Rules.Instance.isWhite == true) || (Rules.Instance.chess.board.moveNumber % 2 != 0 && Rules.Instance.isWhite == false))      
            return;

        pickPosition = clickedItem.position;//Сохранение координат куда кликнули


        string move = Rules.Instance.chess.GetFigureAt((int)(pickPosition.x / 2.0), (int)(pickPosition.y / 2.0)).ToString() + Rules.GetSquare(pickPosition);
        Rules.Instance.MarkSelectedFigureMoves(move);

        item = clickedItem.gameObject;
        state = State.drag;//смена статуса фигуры на захват
        offset = (Vector2)(clickedItem.position) - clickPosition;//смещение от центра фигуры к тому месту куда мы щёлкнули(чтобы можно было хватать за угол)
    }

    //функция возвращающая координату,где мы нажали ЛКМ
    Vector2 GetClickPosition()
    {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);//конвертация координат экранные(камеры) в мировые
    }

    //функция возвращает объект на который мы попали при щелчке
    //Transform все осн. признаки объекта поворот размер поз. и т.д.
    Transform GetItemAt(Vector2 position)
    {
        //Physics2D проверяет какие калайдеры задействованы в указанной позиции
        //Raycast массив из выделенных элементов 
        RaycastHit2D[] figures = Physics2D.RaycastAll(position, position, 0.5f);//функция вернёт объект,который находится в этих координатах с радиусом 0.5f
        if (figures.Length == 0)
            return null;
        return figures[0].transform;//Transform(все основные признаки объекта поворот позиция и т.д.)
    }

    //Функция присваивания схваченной фигуре позиции курсора мыши
    void Drag()
    {
        item.transform.position = GetClickPosition() + offset;
    }
    void Drop()
    {
        dropPosition = item.transform.position;
        for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
                Rules.Instance.MarkSquare(x, y, false);

        state = State.none;
        item = null;
    }

    


}

