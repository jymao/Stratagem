using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//C# HAS NO PRIORITY QUEUE?!?!?!
public class PriorityQueue {

    private List<Vector3> minHeap; // x is row, y is col, z is cost

    public PriorityQueue()
    {
        minHeap = new List<Vector3>();
        minHeap.Add(new Vector3()); //index 0 not used
    }

    public int Count()
    {
        return minHeap.Count - 1; //index 0 not used
    }

    public void Insert(Vector3 node)
    {
        //add new value to end of list
        minHeap.Add(node);

        int currIndex = Count();

        //traverse up until root (index 1) and bubble up entry if necessary
        while(ParentIndex(currIndex) >= 1)
        {
            if(minHeap[ParentIndex(currIndex)].z > minHeap[currIndex].z)
            {
                Swap(ParentIndex(currIndex), currIndex);
                currIndex = ParentIndex(currIndex);
            }
            else
            {
                break;
            }
        }
    }

    public Vector3 RemoveMin()
    {
        Vector3 result = minHeap[1];

        //reorganize heap
        if (Count() > 1)
        {
            minHeap[1] = minHeap[Count()]; //replace root with rightmost leaf
            minHeap.RemoveAt(Count());

            int currIndex = 1;

            //traverse down, if there are any children, must have left child before right child for heap property
            while(LeftChildIndex(currIndex) <= Count())
            {
                //has both left and right child
                if(RightChildIndex(currIndex) <= Count())
                {
                    float leftCost = minHeap[LeftChildIndex(currIndex)].z;
                    float rightCost = minHeap[RightChildIndex(currIndex)].z;

                    //if parent is larger than any children, swap with smaller child
                    if(minHeap[currIndex].z > leftCost || minHeap[currIndex].z > rightCost)
                    {
                        if(leftCost < rightCost)
                        {
                            Swap(currIndex, LeftChildIndex(currIndex));
                            currIndex = LeftChildIndex(currIndex);
                        }
                        else
                        {
                            Swap(currIndex, RightChildIndex(currIndex));
                            currIndex = RightChildIndex(currIndex);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                //has only left child, making the child a leaf node
                else
                {
                    if(minHeap[LeftChildIndex(currIndex)].z < minHeap[currIndex].z)
                    {
                        Swap(currIndex, LeftChildIndex(currIndex));
                    }

                    break;
                }
            }
        }
        else if(Count() == 1)
        {
            minHeap.RemoveAt(1);
        }

        return result;
    }

    private int LeftChildIndex(int index)
    {
        return index * 2;
    }

    private int RightChildIndex(int index)
    {
        return index * 2 + 1;
    }

    private int ParentIndex(int index)
    {
        return index / 2;
    }

    private void Swap(int parentIndex, int childIndex)
    {
        Vector3 temp = minHeap[parentIndex];
        minHeap[parentIndex] = minHeap[childIndex];
        minHeap[childIndex] = temp;
    }

    public void DebugPrint()
    {
        for(int i = 1; i <= Count(); i++)
        {
            Debug.Log(minHeap[i]);
        }
    }
}
