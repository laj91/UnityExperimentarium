Turret prefab består af:
TurretBase
└── TurretPivot (med Rigidbody og Configurable Joint)
    └── TurretBody (mesh + collider + Rigidbody)
        ├── RightGrabPoint (XRGrabInteractable)
        ├── LeftGrabPoint (XRGrabInteractable eller Secondary)
        └── FirePoint (Transform i fronten)