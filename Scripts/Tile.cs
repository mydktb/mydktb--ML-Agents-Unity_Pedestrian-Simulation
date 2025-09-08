using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    
    [SerializeField] Color _baseColor, _offsetColor;
    [SerializeField] Renderer _renderer;
    [SerializeField] public GameObject[] _highlights;
    [SerializeField] float seconds = 50f;
    [SerializeField] string _tag =  "tile"; 
    [SerializeField] string _YTag = "m_YellowVisitedTile";
    [SerializeField] string _BTag = "m_BellowVisitedTile";
    [SerializeField] string _RTag = "m_RellowVisitedTile";


    //Renderer _Renderer;
    //[SerializeField] Material _cat1; // Green
    //[SerializeField] Material _cat2; // Blue
    //[SerializeField] Material _cat3; // Red
    //[SerializeField] Material _cat4; // Purple
    //[SerializeField] Material _cat5; // Yellow
    //[SerializeField] Material _cat6; // White


    private void Start()
    {
        //_Renderer = GetComponent<Renderer>();
        
    }
    public void ResetTiles(string tag)
    {
        for (int i = 0; i < _highlights.Length; i++)
        {
            _highlights[i].SetActive(false);
        }
        //for (int i = 0; i < _highlights.Length; i++)
        //{
        //    if (tag == _YTag) _highlights[0].SetActive(false);
        //    if (tag == _BTag) _highlights[1].SetActive(false);
        //    if (tag == _RTag) _highlights[2].SetActive(false);
        //}
    }

    //public void ActivateColor(int index)
    //{
    //    for (int i = 0; i < _highlights.Length; i++)
    //    {
    //        if (i == index)
    //        {
    //            _highlights[i].SetActive(true);
    //        }
    //        else
    //        {
    //            _highlights[i].SetActive(false);

    //        }
    //    }

    //}

    public void Init(bool isOffset)
    {
        _renderer.material.color = isOffset ? _offsetColor : _baseColor;
    }
    private void OnTriggerExit(Collider other)
    {

        if (other.gameObject.layer == 25) _highlights[0].SetActive(true);
        if (other.gameObject.layer == 26) _highlights[1].SetActive(true);
        if (other.gameObject.layer == 27) _highlights[2].SetActive(true);
        if (other.gameObject.layer == 28) _highlights[3].SetActive(true);
        if (other.gameObject.layer == 29)
        {
            _highlights[4].SetActive(true);
            _highlights[4].tag = _YTag;
        }
        
        if (other.gameObject.layer == 30)
        {
            _highlights[5].SetActive(true);
            _highlights[5].tag = _YTag;

        }
        if (other.gameObject.layer == 31)
        {
            _highlights[6].SetActive(true);
            _highlights[6].tag = _YTag;

        }


        //if (other.gameObject.layer == 20)
        //{

        //    _highlights[0].SetActive(true);
        //    //_highlights[1].SetActive(false);
        //    //_highlights[2].SetActive(false);

        //    _highlights[0].tag = _YTag;
        //    _highlights[1].tag = _tag;
        //    _highlights[2].tag = _tag;

        //   //StartCoroutine(Clear(seconds));
        //}

        //if (other.gameObject.layer == 21)
        //{

        //    _highlights[1].SetActive(true);

        //    _highlights[0].tag = _tag;
        //    _highlights[1].tag = _BTag;
        //    _highlights[2].tag = _tag;

        //    //StartCoroutine(Clear(seconds));

        //}

        //if (other.gameObject.layer == 22)
        //{
        //    _highlights[0].SetActive(false);
        //    _highlights[1].SetActive(false);
        //    _highlights[2].SetActive(true);

        //    _highlights[0].tag = _tag;
        //    _highlights[1].tag = _tag;
        //    _highlights[2].tag = _RTag;

        //    //StartCoroutine(Clear(seconds));
        //}
    }

    //private void OnCollisionExit(Collision collision)
    //{

    //    if (collision.gameObject.layer == 20)
    //    {
    //        _visitedYellow = true;
    //        _visitedBlue = false;
    //        _visitedRed = false;

    //        _highlights[0].SetActive(true);
    //        _highlights[1].SetActive(false);
    //        _highlights[2].SetActive(false);

    //        _highlights[0].tag = _YTag;
    //        _highlights[1].tag = _tag;
    //        _highlights[2].tag = _tag;

    //        StartCoroutine(Clear(seconds));
    //    }

    //    if (collision.gameObject.layer == 21)
    //    {

    //        _visitedYellow = false;
    //        _visitedBlue = true;
    //        _visitedRed = false;

    //        _highlights[0].SetActive(false);
    //        _highlights[1].SetActive(true);
    //        _highlights[2].SetActive(false);

    //        _highlights[0].tag = _tag;
    //        _highlights[1].tag = _BTag;
    //        _highlights[2].tag = _tag;

    //        StartCoroutine(Clear(seconds));

    //    }

    //    if (collision.gameObject.layer == 22)
    //    {

    //        _visitedYellow = false;
    //        _visitedBlue = false;
    //        _visitedRed = true;

    //        _highlights[0].SetActive(false);
    //        _highlights[1].SetActive(false);
    //        _highlights[2].SetActive(true);

    //        _highlights[0].tag = _tag;
    //        _highlights[1].tag = _tag;
    //        _highlights[2].tag = _RTag;

    //        StartCoroutine(Clear(seconds));

    //    }
    //}

    //IEnumerator Clear(float seconds)
    //{
    //    yield return new WaitForSeconds(seconds);
    //    for (int i = 0; i < _highlights.Length; i++)
    //    {
    //        _highlights[i].SetActive(false);
    //        _highlights[i].tag = "tile";
    //        _visitedYellow = false;
    //        _visitedBlue = false;
    //        _visitedRed = false;
    //    }
    //}
}
