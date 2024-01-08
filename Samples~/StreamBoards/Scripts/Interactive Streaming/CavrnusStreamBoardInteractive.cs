using UnityEngine;

namespace CavrnusSdk.StreamBoards
{
    [RequireComponent(typeof(CavrnusStreamBoard))]
    public class CavrnusStreamBoardInteractive : MonoBehaviour
    {
        [SerializeField] private CavrnusStreamContextMenu ctxMenuPrefab;

        private CavrnusStreamBoard board;
        private CavrnusSpaceConnection spaceConn;

        private void Awake()
        {
            board = GetComponent<CavrnusStreamBoard>();
        }
        
        private void Start()
        {
            CavrnusSpaceJoinEvent.OnAnySpaceConnection(csc => {
                spaceConn = csc;
            });
        }

        public void SelectStreamBoard()
        {
            if (spaceConn == null) 
                return;
            
            var ctx = Instantiate(ctxMenuPrefab, null);
            
            ctx.GetComponentInChildren<CavrnusStreamContextMenu>().Setup(spaceConn.UsersList.Users, user => {
                board.UpdateAndBindUserTexture(user);

                Destroy(ctx.gameObject);
            });
        }
    }
}