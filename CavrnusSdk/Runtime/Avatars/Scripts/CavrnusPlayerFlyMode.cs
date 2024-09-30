using UnityEngine;

namespace CavrnusSdk.Avatars
{
    public class CavrnusPlayerFlyMode : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float lookSpeedX = 2f;
        [SerializeField] private float lookSpeedY = 2f;

        private bool isRotating = false;

        private void Update()
        {
            if (Input.GetMouseButtonDown(1)) {
                isRotating = true;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            if (Input.GetMouseButtonUp(1)) {
                isRotating = false;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            if (isRotating) {
                var rotationX = Input.GetAxis("Mouse X") * lookSpeedX;
                var rotationY = -Input.GetAxis("Mouse Y") * lookSpeedY;

                transform.Rotate(Vector3.up, rotationX, Space.World);
                transform.Rotate(Vector3.right, rotationY, Space.Self);
            }

            var translationX = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;
            var translationZ = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;

            var translationY = 0f;
            if (Input.GetKey(KeyCode.E))
                translationY = moveSpeed * Time.deltaTime;
            else if (Input.GetKey(KeyCode.Q)) translationY = -moveSpeed * Time.deltaTime;

            transform.Translate(new Vector3(translationX, translationY, translationZ), Space.Self);
        }
    }
}