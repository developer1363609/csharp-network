using System.Collections;

namespace network.Utils;

public class ListEnumerator<T2> : IEnumerator<T2>
{
    public static readonly XPool<ListEnumerator<T2>> sPool
        = new XPool<ListEnumerator<T2>>();

    private XList<T2> mParent;

    private int mCurIdx = -1;

    private int mTotal = 0;

    public ListEnumerator()
    {
        mParent = null;
        mTotal = 0;
        mCurIdx = -1;
    }

    public void SetList(XList<T2> L)
    {
        mParent = L;
        mTotal = L.Count;
        mCurIdx = -1;
    }

    public T2 Current
    {
        get
        {
            return mParent[mCurIdx];
        }
    }

    object IEnumerator.Current
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    public void Dispose()
    {
        mParent = null;//消除引用
        sPool.Put(this);
    }

    public bool MoveNext()
    {
        ++mCurIdx;
        return mCurIdx < mTotal;
    }

    public void Reset()
    {
        mCurIdx = -1;
        mTotal = mParent.Count;
    }

    
}