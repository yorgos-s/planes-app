using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//////////////////////////////////////////////////////////////////////////
// This Class has a very important Method
// SequencePointRoad --> This method finds  A L L  the sequential points for a given road
// This means that "SequencePointRoad" reconstructs a given road as a series of consequtive NP+non-NP pointsd
// (the start of the road -which is very tricky- calculated from StartPointRoad)

//For instance I road has   C D4 I    E I    I NP (1,2,3)   G H I

// Notice that a road can have multiple NP's in different parts of the sequence
// In other class we cut off a spesific part of the road (at his Non-NP's points)
// BUT SequencePointRoad, finds  A  L  L    the consecutive points for a given road   NP's + NON NP's


//////////////////////////////////////////////////////////////////////////

public class FindSequence
{

    Separate separate = new Separate();
    PathInfo pathInfo = new PathInfo();
    //public Transform transform;
    GameObject pathObj;
    public Dictionary<string, List<Transform>> dictPathRoads = new Dictionary<string, List<Transform>>();

    public void Start_FindSequence()
    {
        
        pathObj = GameObject.Find("Path");
        pathInfo.StartPathObjInfo();

        foreach (string name in pathInfo.DifferentNames())  //difNamesList
        {
            
            dictPathRoads.Add(name, SequencePointsRoad(name)); // this never started w/o  pathInfo.StartPathObjInfo();
            // You retreive it by simply put dictPathRoads["C"]
        }

      

    }


    /*

       _____ _             _     _____      _       _     _____                 _ 
      / ____| |           | |   |  __ \    (_)     | |   |  __ \               | |
     | (___ | |_ __ _ _ __| |_  | |__) |__  _ _ __ | |_  | |__) |___   __ _  __| |
      \___ \| __/ _` | '__| __| |  ___/ _ \| | '_ \| __| |  _  // _ \ / _` |/ _` |
      ____) | || (_| | |  | |_  | |  | (_) | | | | | |_  | | \ \ (_) | (_| | (_| |
     |_____/ \__\__,_|_|   \__| |_|   \___/|_|_| |_|\__| |_|  \_\___/ \__,_|\__,_|


     * */
    // FEATURE IN THE FUTURE 
    //IF THE CODE BELOW FAILS --> OPTION FOR MANUAL POINTS

    // find X or Y
    public Transform StartPointRoad(string roadName)
    {

        float difY;
        float difX;

        //bool XorY;

        List<float> roadX = new List<float>();
        List<float> roadY = new List<float>();
        List<Transform> TransformsInTheRoad = new List<Transform>();

        List<Transform> possibleStartX = new List<Transform>();
        List<Transform> possibleStartY = new List<Transform>();

        // AllChildrenTransforms = GetComponentsInChildren<Transform>();
        foreach (Transform childTransform in pathObj.transform)
        {
            // Debug.Log(childTransform.name);
            if (separate.FromSpaces(childTransform.name).Contains(roadName))
            {
                // Debug.Log("Contains D8!");
                roadX.Add(childTransform.position.x);
                roadY.Add(childTransform.position.y);

                TransformsInTheRoad.Add(childTransform);

            }
        }

        // find range X 
        difX = Mathf.Max(roadX.ToArray()) - Mathf.Min(roadX.ToArray());
        // find range Y
        difY = Mathf.Max(roadY.ToArray()) - Mathf.Min(roadY.ToArray());

        // The result of ranges => is X or Y oriented
        bool absDif;
        absDif = Mathf.Abs(difX) > Mathf.Abs(difY);



        List<Transform> equalPointsInTheRoad = new List<Transform>();
        // is X or Y oriented??????
        if (absDif)
        {

            foreach (Transform pointTransform in TransformsInTheRoad)
            {

                //  _____________________________
                //  |A2                         A3
                //  |
                //  |
                //  |A1                for this shape (X oriented) we have 2 --> A1, A2

                // how many equal mins exist in X axis? -->
                //We search how many transforms from a road are approximatelly equal
                if (AproximatellyEqual(pointTransform.position.x, Mathf.Min(roadX.ToArray()), 0.1f))
                {
                    //Debug.Log(pointTransform.name); 
                    equalPointsInTheRoad.Add(pointTransform);
                }
            }  // end foreach  X 
        }
        else
        {

            foreach (Transform pointTransform in TransformsInTheRoad)
            {
                // find HOW MANY Y with approximately the same Y exist AS MINIMUM ONLY!
                if (AproximatellyEqual(pointTransform.position.y, Mathf.Min(roadY.ToArray()), 0.1f))
                {
                    //Debug.Log(roadTransform.name);
                    equalPointsInTheRoad.Add(pointTransform);
                }
            } // end foreach  Y
        } // END IF

        // -----------------------------------------------------

        // FIND THE CLOSEST --> find all the points that are NOT equals
        int nrCounts = 0; // for debugging delete
        List<Transform> nonEqualPointsInTheRoad = new List<Transform>();

        foreach (Transform pointTransform in TransformsInTheRoad)
            if (!ContainOneTransformFromList(pointTransform, equalPointsInTheRoad)) //equalPointsInTheRoad
            {
                //ClosestFromList(TransformsInTheRoad[0], TransformsInTheRoad);
                nrCounts++;
                //Debug.Log(pointTransform.name);
                nonEqualPointsInTheRoad.Add(pointTransform);
            }

        // We use the first equal point equalPointsInTheRoad[0] in the list as a reference START point
        //(We could use equalPointsInTheRoad[1]  as ALL THE EQUAL POINTS GIVE SAME RESULT)
        // Debug.Log(ClosestFromList(equalPointsInTheRoad[0], nonEqualPointsInTheRoad));
        Transform pointAfterSlope = ClosestFromList(equalPointsInTheRoad[0], nonEqualPointsInTheRoad);
        Transform StartPoint = FurthestFromList(pointAfterSlope, equalPointsInTheRoad);

        //Debug.Log(StartPoint);



        return StartPoint; // just for work until completion
    }

    /*//////////////////////////////////////////////////////////////////////////////
     *  /////////////////////////////////////////////////////////////////////////////
     *                    END StartPointRoad
     *  /////////////////////////////////////////////////////////////////////////////
     *  /////////////////////////////////////////////////////////////////////////////
     */



    /*//////////////////////////////////////////////////////////////////////////////
     *  /////////////////////////////////////////////////////////////////////////////
     *                    METHODS FOR StartPointRoad
     *  /////////////////////////////////////////////////////////////////////////////
     *  /////////////////////////////////////////////////////////////////////////////
     */


    //is a==b? --> is a==b (+-x)?  
    bool AproximatellyEqual(float numberTest, float numberLimit, float range)
    {
        return (numberTest > numberLimit - range && numberTest < numberLimit + range);

    }


    // find closest Transform (Transform WITH the smallest magnitude)
    Transform ClosestFromList(Transform startPoint, List<Transform> ListPoints)
    {
        List<float> magnitudes = new List<float>();
        Dictionary<float, Transform> magnitudesDict = new Dictionary<float, Transform>();

        foreach (Transform endPoint in ListPoints)
        {
            // to find the the vector of two points you have to subtract them      
            //magnitudesAL.Add((endPoint.position - startPoint.position).magnitude);       
            magnitudes.Add((endPoint.position - startPoint.position).magnitude);

            //hash table -n order to find the corresponding Transform
            magnitudesDict.Add((endPoint.position - startPoint.position).magnitude, endPoint);
        }

        float min = Mathf.Min(magnitudes.ToArray());
        //Debug.Log(min);
        Transform ClosestPoint = magnitudesDict[min];


        return ClosestPoint;
    }


    Transform FurthestFromList(Transform startPoint, List<Transform> ListPoints)
    {
        List<float> magnitudes = new List<float>();
        Dictionary<float, Transform> magnitudesDict = new Dictionary<float, Transform>();
        //ArrayList magnitudesAL = new ArrayList();

        foreach (Transform endPoint in ListPoints)
        {
            magnitudes.Add((endPoint.position - startPoint.position).magnitude);
            //hash table -n order to find the corresponding Transform
            magnitudesDict.Add((endPoint.position - startPoint.position).magnitude, endPoint);
        }

        float max = Mathf.Max(magnitudes.ToArray());
        //Debug.Log(max);
        Transform FurthestPoint = magnitudesDict[max];
        //Debug.Log(FurthestPoint);

        //magnitudesDict.Add(endPoint, (endPoint.position - startPoint.position).magnitude);
        // foreach (KeyValuePair<float,Transform> Magnitude in magnitudesDict) { }

        return FurthestPoint;
    }



    // .Contain(List<string>)   (normally is   .Contain("someText")  )
    bool ContainOneTransformFromList(Transform testTransform, List<Transform> unfoldTransforms)
    {
        List<bool> boolEqualOrNotList = new List<bool>();

        bool ContainsJustOne = false;

        foreach (Transform eachTransform in unfoldTransforms)
        {
            if (testTransform == eachTransform)
            {
                ContainsJustOne = true;
            }
        }
        return ContainsJustOne;
    }



    /*//////////////////////////////////////////////////////////////////////////////
    *  /////////////////////////////////////////////////////////////////////////////
    *             E  N   D     METHODS FOR StartPointRoad
    * /////////////////////////////////////////////////////////////////////////////
    *//////////////////////////////////////////////////////////////////////////////






    //   SequencePointRoad   all it does is to find   ALL the sequenctial points from start to end
    //  A =  A A1   A A2   A A3   A A4   A A5   A A6   A A7   A A8   A A9   A A11   A A13 A14   


    /*//////////////////////////////////////////////////////////////////////////////
    *  /////////////////////////////////////////////////////////////////////////////
    *             METHODS FOR POINT SEQUENCE
    *  /////////////////////////////////////////////////////////////////////////////
    *  /////////////////////////////////////////////////////////////////////////////
    */



    // List Transforms of all points with the name "I"  etc
    List<Transform> ThisRoadListTransforms(string roadName)
    {
        List<Transform> TransformsInThisRoad = new List<Transform>();
        foreach (Transform childTransform in pathObj.transform)
        {
            if (separate.FromSpaces(childTransform.name).Contains(roadName))
            {
                TransformsInThisRoad.Add(childTransform);
            }
        }
        return TransformsInThisRoad;
    }








    /*         
  _____    ____  _____  _   _  _______        _____  ______  ____   _    _  ______  _   _   _____  ______ 
 |  __ \  / __ \|_   _|| \ | ||__   __|      / ____||  ____|/ __ \ | |  | ||  ____|| \ | | / ____||  ____|
 | |__) || |  | | | |  |  \| |   | |        | (___  | |__  | |  | || |  | || |__   |  \| || |     | |__   
 |  ___/ | |  | | | |  | . ` |   | |         \___ \ |  __| | |  | || |  | ||  __|  | . ` || |     |  __|  
 | |     | |__| |_| |_ | |\  |   | |         ____) || |____| |__| || |__| || |____ | |\  || |____ | |____ 
 |_|      \____/|_____||_| \_|   |_|        |_____/ |______|\___\_\ \____/ |______||_| \_| \_____||______|
                                                                                                                                                                                                                  
      */

    // FIND FOR A GIVEN START AND A GIVEN ROAD   A  L  L   THE SEQUENTIAL POINTS
    Transform NextPointTransformRoad(string roadName, List<Transform> previousPoints)
    {
        // Transform startPoint = StartPointRoad(roadName);
        List<Transform> remainPoints = ThisRoadListTransforms(roadName);
        foreach (Transform pointTransform in previousPoints)
        {
            remainPoints.Remove(pointTransform);
        }
        foreach (Transform pointTransform in remainPoints)
        {
            //Debug.Log(pointTransform);
        }

        List<float> magnitudes = new List<float>();
        // Dict is a hack way to take later on Tranform BY min-magnitude
        Dictionary<float, Transform> magnitudesDict = new Dictionary<float, Transform>();

        float testedMinMagnitude;


        Transform lastPreviousPoint = previousPoints[(previousPoints.Count) - 1];

        foreach (Transform point in remainPoints)
        {
            // find the magnitude between each point in the remain points  and the LAST FOUND POINT
            testedMinMagnitude = (point.position - lastPreviousPoint.position).magnitude;
            //???WTH?? testedMinMagnitude = (point.position - remainPoints[remainPoints.Count].position).magnitude;
            magnitudes.Add(testedMinMagnitude);
            magnitudesDict.Add(testedMinMagnitude, point);
        }


        float min = Mathf.Min(magnitudes.ToArray());
        Transform ClosestPoint = magnitudesDict[min];

        return ClosestPoint;
    }


    //*******************************************************************************************************************

    //NextPointTransformRoad(string roadName, List<Transform> previousPoints)
    // each new point you find add it to list
    public List<Transform> SequencePointsRoad(string roadName)
    {
        List<Transform> pathList = new List<Transform>();

        List<Transform> previousPointsSeq = new List<Transform>();
        Transform nextPoint;


        for (int i = 0; i < ThisRoadListTransforms(roadName).Count; i++) //
        {
            //Debug.Log(ThisRoadListTransforms(roadName).Count);

            if (i == 0)
            {
                previousPointsSeq.Add(StartPointRoad(roadName));
                //Transform startPoint = NextPointTransformRoad(roadName, previousPointsSeq); 
            }
            else
            {
                nextPoint = NextPointTransformRoad(roadName, previousPointsSeq);
                previousPointsSeq.Add(nextPoint);
            }
        }

        return previousPointsSeq;
    }












}
