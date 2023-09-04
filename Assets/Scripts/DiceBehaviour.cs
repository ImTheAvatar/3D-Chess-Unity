
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class DiceBehaviour :MonoBehaviour
{
    public int Dice1,Dice2;
    public bool Locked;
    public static DiceBehaviour Instance;
    [SerializeField] Image dice1Im,dice2Im;
    [SerializeField] List<Sprite> numbers;
    private void Start()
    {
        Instance= this;
        dice1Im.sprite=null; dice2Im.sprite=null;
    }
    public async Task<bool> MakeRandomDice()
    {
        Locked = true;
        for (int i = 0; i < numbers.Count; i++)
        {
            dice1Im.sprite = numbers[Random.Range(0, 6)];
            dice2Im.sprite = numbers[Random.Range(0, 6)];
            await Task.Delay(100);
        }
        Dice1 = Random.Range(0, 6);
        Dice2 = Random.Range(0, 6);

        dice1Im.sprite = numbers[Dice1];
        dice2Im.sprite = numbers[Dice2];
        Dice1++;
        Dice2++;
        Locked = false;
        return true;
    }
}
