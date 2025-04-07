using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MemoryPool
{
    private class PoolItem
    {
        public bool isActive; // GameObject 활성화, 비활성화 정보
        public GameObject gameObject; // 화면에 보이는 실제 게임오브젝트
    }

    private int increaseCount = 5; // 오브젝트가 부족시 Instantiate()로 추가될 오브젝트 개수
    private int maxCount; // 현재 리스트에 등록된 오브젝트 개수
    private int activeCount; // 현재 게임에 있는 오브젝트 개수

    private GameObject poolObject;
    private List<PoolItem> poolItemList;

    public MemoryPool(GameObject poolObject)
    {
        maxCount = 0;
        activeCount = 0;
        this.poolObject = poolObject;
        poolItemList = new List<PoolItem>();

        InstantiateObject();
    }

    public void InstantiateObject()
    {
        maxCount += increaseCount;
        for(int i =0; i < increaseCount; ++i)
        {
            PoolItem poolItem = new PoolItem();
            poolItem.isActive = false;
            poolItem.gameObject = GameObject.Instantiate(poolObject);
            poolItem.gameObject.SetActive(false);
            poolItemList.Add(poolItem);
        }
    }

    //현재 관리중인 모든 오브젝트 삭제
    public void DestoryObjects()
    {
        if (poolItemList == null) return;
        int count = poolItemList.Count;
        for(int i =0; i < count; ++i)
        {
            GameObject.Destroy(poolItemList[i].gameObject);
        }
        poolItemList.Clear();
    }

    //PoolList에 저장된 오브젝트를 활성화해 사용
    //InstantiateObjects()로 추가 생성
    public GameObject ActivePoolItem()
    {
        if (poolItemList == null) return null;
        if(maxCount == activeCount)
        {
            InstantiateObject();
        }
        int count = poolItemList.Count;
        for(int i =0; i < count; ++i)
        {
            PoolItem poolItem = poolItemList[i];

            if(poolItem.isActive == false)
            {
                activeCount++;
                poolItem.isActive = true;
                poolItem.gameObject.SetActive (true);
                return poolItem.gameObject;
            }
        }
        return null;
    }

    //게임에 사용 완료한 오브젝트를 비활성화
    public void DeactivatePoolItems(GameObject removeObject)
    {
        if (poolItemList == null|| removeObject ==null ) return;
 
        int count = poolItemList.Count;
        for (int i = 0; i < count; ++i)
        {
            PoolItem poolItem = poolItemList[i];

            if (poolItem.gameObject== removeObject)
            {
                activeCount--;
                poolItem.isActive = false;
                poolItem.gameObject.SetActive(false);
            }
        }
        activeCount = 0;
    }



    //게임에 사용중인 모든 오브젝트를 비활성화 상태로 설정
    public void DeactivateAllPoolItems()
    {
        if (poolItemList == null) return;
        int count = poolItemList.Count;
        for(int i =0; i < count; ++i)
        {
            PoolItem poolItem = poolItemList[i];

            if(poolItem.gameObject != null && poolItem.isActive == true)
            {
                poolItem.isActive = false;
                poolItem.gameObject.SetActive (false);
            }
        }
        activeCount = 0;
    }

}
