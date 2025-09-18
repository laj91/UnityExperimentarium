//using UnityEngine;
//using Cinemachine;
//using StarterAssets;
//using UnityEngine.Animations.Rigging;

//public class ThirdPersonShooterController : MonoBehaviour
//{
//    [SerializeField] CinemachineVirtualCamera virtualCamera;
//    [SerializeField] float normalSensitivity;
//    [SerializeField] float aimSensitivity;
//    [SerializeField] GameObject crosshairCanvas;
//    [SerializeField] LayerMask aimLayerMask;
//    [SerializeField] Transform debugTransform;
//    [SerializeField] GameObject bulletPrefab;
//    [SerializeField] Transform bulletShootPosition;
//    [SerializeField] Transform rightHandTarget;
//    [SerializeField] Transform aimTarget; // Hvor du vil pege
//    [SerializeField] Rig rightArmRig;
//    [SerializeField] StarterAssetsInputs starterAssetInputs;

//    //private StarterAssetsInputs starterAssetInputs;
//    //private Animator animator;
//    // Start is called once before the first execution of Update after the MonoBehaviour is created
//    private void Awake()
//    {
//        //starterAssetInputs = GetComponent<StarterAssetsInputs>();
//        //animator = GetComponent<Animator>();
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        Vector3 mouseWorldPosition = Vector3.zero;
//        //Gets the center of the screen
//        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
//        Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
//        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimLayerMask))
//        {
//            debugTransform.position = raycastHit.point;
//            mouseWorldPosition = raycastHit.point;
//        }
//        if (starterAssetInputs.aim)
//        {
//            virtualCamera.gameObject.SetActive(true);
//            thirdPersoninputs.SetSensitivity(aimSensitivity);
//            thirdPersoninputs.SetRotationMove(true);
//            crosshairCanvas.SetActive(true);

//            //animator.SetLayerWeight(1, Mathf.Lerp(animator.GetLayerWeight(1), 1f, Time.deltaTime * 10f));

//            //Rotate the player when aiming
//            Vector3 worldAimTarger = mouseWorldPosition;
//            worldAimTarger.y = transform.position.y;
//            Vector3 aimDirection = (worldAimTarger - transform.position).normalized;
//            transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 20f);

//            rightHandTarget.position = aimTarget.position;
//            rightArmRig.weight = Mathf.Lerp(rightArmRig.weight, 1f, Time.deltaTime * 5f);
//        }
//        else
//        {
//            virtualCamera.gameObject.SetActive(false);
//            thirdPersoninputs.SetSensitivity(normalSensitivity);
//            thirdPersoninputs.SetRotationMove(true);
//            crosshairCanvas.SetActive(false);
//            //animator.SetLayerWeight(1, Mathf.Lerp(animator.GetLayerWeight(1), 0f, Time.deltaTime * 10f));
//            rightArmRig.weight = Mathf.Lerp(rightArmRig.weight, 0f, Time.deltaTime * 5f);
//        }

//        if (starterAssetInputs.shoot)
//        {
//            Vector3 aimDirection = (mouseWorldPosition - bulletShootPosition.position).normalized;
//            Instantiate(bulletPrefab, bulletShootPosition.position, Quaternion.LookRotation(aimDirection, Vector3.up));
//            starterAssetInputs.shoot = false;
//        }
//    }
//}
