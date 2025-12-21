<img src="https://r2cdn.perplexity.ai/pplx-full-logo-primary-dark%402x.png" style="height:64px;margin-right:32px"/>

# no, give me the mongodb collections to create for earlier opt \& webauth code which yopu provided

For the earlier OTP + WebAuthn code, you need three MongoDB collections:

## users collection

Stores one document per user, keyed by phone number and containing any WebAuthn credentials.

```js
db.createCollection("Users");

db.Users.createIndex({ mobile: 1 }, { unique: true });

/* Example document */
{
  _id: ObjectId("675ffc42b3e3f0e9a7a1b123"),
  mobile: "+919876543210",
  createdAt: ISODate("2025-12-17T12:00:00Z"),
  lastLoginAt: ISODate("2025-12-17T14:30:00Z"),
  status: "active",
  webAuthnCredentials: [
    {
      credentialId: "lWZgjEJ5rR5ry3-3E4d2xQ",   // base64url
      publicKey: "pQECAyYgASFYIE0e...snip...", // base64/COSE
      signCount: 0,
      userAgent: "Chrome 131 / Android 15",
      deviceName: "Pixel 9 Pro",
      createdAt: ISODate("2025-12-17T12:10:00Z"),
      lastUsedAt: ISODate("2025-12-17T12:10:00Z")
    }
  ]
}
```

Recommended extra index to speed WebAuthn lookup:

```js
db.Users.createIndex(
  { "webAuthnCredentials.credentialId": 1 },
  { name: "idx_user_webauthn_credentialId" }
);
```

This aligns with typical WebAuthn key storage in MongoDB.[^1][^2][^3]

## OtpSessions collection

Backs the `OtpService` used by `/auth/send-otp` and `/auth/verify-otp`.

```js
db.createCollection("OtpSessions");

/* Example document */
{
  _id: ObjectId("675ffd12b3e3f0e9a7a1b999"),
  mobile: "+919876543210",
  otpHash: "pP5C7cYo3+...base64 sha256...",
  expiresAt: ISODate("2025-12-17T12:05:00Z"),
  consumed: false
}
```

Useful indexes:

```js
// Find active sessions by mobile quickly
db.OtpSessions.createIndex({ mobile: 1, expiresAt: -1 });

// TTL cleanup of old records (optional – run on expiresAt)
db.OtpSessions.createIndex({ expiresAt: 1 }, { expireAfterSeconds: 0 });
```

This matches the model in the earlier C\# `OtpSession` class.[^4][^5][^6]

## (Optional) webAuthnCredentials collection

Only needed if you prefer credentials separate from `Users`. If you keep the embedded array, you can skip this.

```js
db.createCollection("WebAuthnCredentials");

/* Example document */
{
  _id: ObjectId("675ffce9b3e3f0e9a7a1b456"),
  userId: ObjectId("675ffc42b3e3f0e9a7a1b123"),
  credentialId: "lWZgjEJ5rR5ry3-3E4d2xQ",
  publicKey: "pQECAyYgASFYIE0e...snip...",
  signCount: 0,
  deviceName: "Pixel 9 Pro",
  userAgent: "Chrome 131 / Android 15",
  createdAt: ISODate("2025-12-17T12:10:00Z"),
  lastUsedAt: ISODate("2025-12-17T12:10:00Z")
}
```

Indexes:

```js
db.WebAuthnCredentials.createIndex({ credentialId: 1 }, { unique: true });
db.WebAuthnCredentials.createIndex({ userId: 1 });
```

Use either:

- `Users` + `OtpSessions` (with `webAuthnCredentials` embedded) – simplest for your existing C\# models, or
- `Users` + `OtpSessions` + `WebAuthnCredentials` – if you want credentials in their own collection.[^2][^3][^1]

<div align="center">⁂</div>

[^1]: https://www.corbado.com/blog/passkey-webauthn-database-guide

[^2]: https://dev.to/corbado/webauthn-guide-for-mysql-postgresql-sqlite-mongodb-5h48

[^3]: https://www.corbado.com/blog/passkey-storage-database-recommendations-webauthn

[^4]: https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-mongo-app?view=aspnetcore-9.0

[^5]: https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-mongo-app?view=aspnetcore-10.0

[^6]: https://codewithmukesh.com/blog/mongodb-in-aspnet-core/

