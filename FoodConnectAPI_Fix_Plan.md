# FoodConnectAPI — Fix Implementation Plan

Based on the [codebase audit](file:///C:/Users/Nguyen/.gemini/antigravity-ide/brain/e37807aa-535c-49df-a9db-05cce759cbd9/codebase_audit.md).

---

---

# Section 1 — [Fixed] Security Fixes

> **Scope:** Authentication/authorization gaps that allow unauthorized data mutation.

---

## 1.1 Add `[Authorize]` + ownership check to `CommentsController`

Currently anyone — even unauthenticated — can edit or delete any comment.

| File | Change |
|------|--------|
| [CommentsController.cs](file:///c:/Practice/FoodConnectAPI/FoodConnectAPI/Controllers/CommentsController.cs) | Add `[Authorize]`, read `ClaimTypes.NameIdentifier`, verify ownership before service call |

**Details:**
- `PATCH /{commentId}` — add `[Authorize]`, call `GetCommentByIdAsync` first, compare `comment.UserId` to the token's `userId`, return `403` if mismatch.
- `DELETE /{commentId}` — same pattern.
- Remove the large dead commented-out code block at lines 17–46.

---

## 1.2 Add role claim to JWT (required for Section 4 — Reports)

The `Role` field exists on `User` but is never included in the JWT, so `[Authorize(Roles = "admin")]` can never work.

| File | Change |
|------|--------|
| [UserService.cs](file:///c:/Practice/FoodConnectAPI/FoodConnectAPI/Services/UserService.cs) | Add `new Claim(ClaimTypes.Role, user.Role)` inside `GenerateJwtToken` |
| [Program.cs](file:///c:/Practice/FoodConnectAPI/FoodConnectAPI/Program.cs) | Confirm `AddAuthorization()` is present (implicit in .NET 8, but make it explicit) |

---

---

# Section 2 — [Fixed] Missing HTTP Endpoints

> **Scope:** Service/repository logic already exists; only controller routes are missing.

---

## 2.1 `GET /api/posts/{postId}` — single post

`GetPostByIdAsync` on `IPostService` is complete. It just needs a route. Note: also see Section 5.1 which fixes the missing `UserName` in its response.

| File | Change |
|------|--------|
| [PostsController.cs](file:///c:/Practice/FoodConnectAPI/FoodConnectAPI/Controllers/PostsController.cs) | Add `GET /{postId}` action with optional `currentUserId` from auth claims |

---

## 2.2 `GET /api/users/{userId}/posts` — posts by a specific user

`GetPostsByUserIdAsync` on `IPostService` is complete.

| File | Change |
|------|--------|
| [PostsController.cs](file:///c:/Practice/FoodConnectAPI/FoodConnectAPI/Controllers/PostsController.cs) | Add `GET /api/users/{userId}/posts` action |

---

## 2.3 `GET /api/users/{userId}` — public user profile

No public profile endpoint exists at all. Requires a new DTO and a new service method.

| File | Change |
|------|--------|
| [NEW] `UserProfileDto.cs` (Models/) | Fields: `Id`, `UserName`, `Region`, `ProfilePictureUrl`, `TotalLikesReceived`, `FollowerCount`, `FollowingCount`, `PostCount` |
| [IUserService.cs](file:///c:/Practice/FoodConnectAPI/FoodConnectAPI/Interfaces/Services/IUserService.cs) | Add `Task<UserProfileDto> GetUserProfileAsync(int userId)` |
| [UserService.cs](file:///c:/Practice/FoodConnectAPI/FoodConnectAPI/Services/UserService.cs) | Implement `GetUserProfileAsync` — fetch user, get follower/following counts from `IFollowRepository`, post count from `IPostRepository`. Inject `IFollowRepository`. |
| [UsersController.cs](file:///c:/Practice/FoodConnectAPI/FoodConnectAPI/Controllers/UsersController.cs) | Add `GET /api/users/{userId}` action |

---

## 2.4 `DELETE /api/users` — delete own account

`UserService.DeleteAsync` is fully implemented. Needs a controller action.  
Also see Section 5.3 which patches the media-file cleanup gap in `DeleteAsync`.

| File | Change |
|------|--------|
| [UsersController.cs](file:///c:/Practice/FoodConnectAPI/FoodConnectAPI/Controllers/UsersController.cs) | Add `[Authorize]` `DELETE /api/users` — read `userId` from claims, look up email from repo, call `DeleteAsync(email)` |

---

---

# Section 3 — 🔴 Follow Feature

> **Scope:** Full end-to-end implementation. Repository layer is complete; service and controller need to be built from scratch.

---

## 3.1 Define the `IFollowService` contract

| File | Change |
|------|--------|
| [IFollowService.cs](file:///c:/Practice/FoodConnectAPI/FoodConnectAPI/Interfaces/Services/IFollowService.cs) | Replace the empty `// Todo` stub with the full interface |

**Methods:**
```csharp
Task FollowUserAsync(int followerId, int followedId);
Task UnfollowUserAsync(int followerId, int followedId);
Task<bool> IsFollowingAsync(int followerId, int followedId);
Task<IEnumerable<FollowUserDto>> GetFollowersAsync(int userId);
Task<IEnumerable<FollowUserDto>> GetFollowingAsync(int userId);
Task<int> GetFollowerCountAsync(int userId);
Task<int> GetFollowingCountAsync(int userId);
```

---

## 3.2 New `FollowUserDto`

| File | Change |
|------|--------|
| [NEW] `FollowUserDto.cs` (Models/) | Fields: `UserId`, `UserName`, `ProfilePictureUrl` |

---

## 3.3 New `FollowService`

| File | Change |
|------|--------|
| [NEW] `FollowService.cs` (Services/) | Full implementation of `IFollowService` |

**Logic:**
- `FollowUserAsync` — guard against self-follow (`followerId == followedId`), check duplicate via `UserIsFollowingAsync`, create + save.
- `UnfollowUserAsync` — call `DeleteFollowByUsersAsync` + save. Throw `KeyNotFoundException` if no follow found.
- `GetFollowersAsync` / `GetFollowingAsync` — map `Follow.Follower` / `Follow.Followed` → `FollowUserDto`.
- Counts — delegate directly to repository.

---

## 3.4 Register and expose Follow endpoints

| File | Change |
|------|--------|
| [Program.cs](file:///c:/Practice/FoodConnectAPI/FoodConnectAPI/Program.cs) | Replace commented-out line with `builder.Services.AddScoped<IFollowService, FollowService>()` |
| [NEW] `FollowsController.cs` (Controllers/) | Four endpoints (see table below) |

**Endpoints:**

| Method | Route | Auth | Action |
|--------|-------|------|--------|
| `POST` | `/api/users/{userId}/follow` | ✅ | Follow user |
| `DELETE` | `/api/users/{userId}/follow` | ✅ | Unfollow user |
| `GET` | `/api/users/{userId}/followers` | — | List followers |
| `GET` | `/api/users/{userId}/following` | — | List following |

---

---

# Section 4 — 🔴 Report Feature

> **Scope:** Full end-to-end implementation. Repository layer is complete; the entity needs a schema change, then service and controller need to be built.

---

## 4.1 Extend the `Report` entity — add `Reason`

The current entity has no reason field, making reports useless in practice.

| File | Change |
|------|--------|
| [Report.cs](file:///c:/Practice/FoodConnectAPI/FoodConnectAPI/Entities/Report.cs) | Add `[MaxLength(500)] public string? Reason { get; set; }` |

**Run new migration:**
```
dotnet ef migrations add AddReasonToReport
dotnet ef database update
```

---

## 4.2 New `ReportAddDto` and `ReportInfoDto`

| File | Change |
|------|--------|
| [NEW] `ReportAddDto.cs` (Models/) | Fields: `Reason` (optional string, max 500) |
| [NEW] `ReportInfoDto.cs` (Models/) | Fields: `Id`, `UserId`, `UserName`, `PostId`, `PostTitle`, `Reason`, `CreatedAt` |

---

## 4.3 Define the `IReportService` contract

| File | Change |
|------|--------|
| [IReportService.cs](file:///c:/Practice/FoodConnectAPI/FoodConnectAPI/Interfaces/Services/IReportService.cs) | Replace the empty `// Todo` stub with the full interface |

**Methods:**
```csharp
Task CreateReportAsync(int userId, int postId, ReportAddDto dto);
Task<bool> DeleteReportAsync(int reportId);
Task<IEnumerable<ReportInfoDto>> GetAllReportsAsync();
Task<IEnumerable<ReportInfoDto>> GetReportsByPostIdAsync(int postId);
```

---

## 4.4 New `ReportService`

| File | Change |
|------|--------|
| [NEW] `ReportService.cs` (Services/) | Full implementation of `IReportService` |

**Logic:**
- `CreateReportAsync` — guard duplicate (`UserHasReportedPostAsync`), map DTO → entity, save.
- `DeleteReportAsync` — call repo delete + save. Return `false` if not found.
- `GetAllReportsAsync` / `GetReportsByPostIdAsync` — map entities → `ReportInfoDto`.

---

## 4.5 Register and expose Report endpoints

| File | Change |
|------|--------|
| [Program.cs](file:///c:/Practice/FoodConnectAPI/FoodConnectAPI/Program.cs) | Add `builder.Services.AddScoped<IReportService, ReportService>()` |
| [NEW] `ReportsController.cs` (Controllers/) | Four endpoints (see table below) |

**Endpoints:**

| Method | Route | Auth | Role | Action |
|--------|-------|------|------|--------|
| `POST` | `/api/posts/{postId}/report` | ✅ | any | Report a post |
| `DELETE` | `/api/reports/{reportId}` | ✅ | admin | Delete a report |
| `GET` | `/api/admin/reports` | ✅ | admin | List all reports |
| `GET` | `/api/admin/reports/post/{postId}` | ✅ | admin | Reports for a post |

> [!IMPORTANT]
> Admin routes require `[Authorize(Roles = "admin")]`. This only works after the role claim is added to the JWT (Section 1.2).

---

---

# Section 5 — 🟠 Bug Fixes

> **Scope:** Logic errors, incomplete data flows, and incorrect DTO mappings.

---

## 5.1 `GetPostByIdAsync` — missing `UserName` in response

`GetAllPostsAsync` populates `UserName`; `GetPostByIdAsync` does not — inconsistent.

| File | Change |
|------|--------|
| [PostService.cs](file:///c:/Practice/FoodConnectAPI/FoodConnectAPI/Services/PostService.cs) | Add `UserName = post.User?.UserName` to the `PostInfoDto` initializer in `GetPostByIdAsync` |
| [PostRepository.cs](file:///c:/Practice/FoodConnectAPI/FoodConnectAPI/Repositories/PostRepository.cs) | Confirm `GetPostByIdAsync` includes `.Include(p => p.User)` — add if missing |

---

## 5.2 Old profile picture not deleted on update

When a user uploads a new profile picture, the old file is orphaned on disk forever.

| File | Change |
|------|--------|
| [UserService.cs](file:///c:/Practice/FoodConnectAPI/FoodConnectAPI/Services/UserService.cs) | In `UpdateProfilePicture`, before setting the new URL: `if (!string.IsNullOrEmpty(user.ProfilePictureUrl)) _fileService.DeleteFile(user.ProfilePictureUrl);` |

---

## 5.3 `DeleteAsync` — user's media files not cleaned up on account deletion

When a user account is deleted, post images and profile pictures stay on disk.

| File | Change |
|------|--------|
| [UserService.cs](file:///c:/Practice/FoodConnectAPI/FoodConnectAPI/Services/UserService.cs) | Inject `IMediaRepository`. Before the transaction commits: collect all `/Uploads` URLs from the user's posts (`Media` table) and profile picture. After commit, delete each via `_fileService.DeleteFile`. |

---

## 5.4 `TotalLikesReceived` — never maintained

The field exists on `User` and is initialized to 0 on registration, but nothing ever increments or decrements it.

| File | Change |
|------|--------|
| [LikeService.cs](file:///c:/Practice/FoodConnectAPI/FoodConnectAPI/Services/LikeService.cs) | Inject `IPostRepository` and `IUserRepository`. In `LikePostAsync`: fetch the post's owner, increment `TotalLikesReceived`, save. In `UnlikePostAsync`: decrement (guard `>= 0`), save. |

---

## 5.5 `UserUpdateDto` — password is required even for non-password updates

`[Required]` on `Password` / `ConfirmPassword` forces users to re-enter their password to change their username or region.

| File | Change |
|------|--------|
| [UserUpdateDto.cs](file:///c:/Practice/FoodConnectAPI/FoodConnectAPI/Models/UserUpdateDto.cs) | Remove `[Required]` from `Password` and `ConfirmPassword` (the service already does `if (!string.IsNullOrWhiteSpace(userUpdateDto.Password)` guard) |

---

## 5.6 Seeder runs in all environments

`DataSeeder.SeedTestDataAsync` runs unconditionally on startup, including production.

| File | Change |
|------|--------|
| [Program.cs](file:///c:/Practice/FoodConnectAPI/FoodConnectAPI/Program.cs) | Wrap seeder call in `if (app.Environment.IsDevelopment()) { ... }` |

---

---

# Section 6 — 🟡 Infrastructure & Code Quality

> **Scope:** Low-risk improvements — no new features, no DB changes.

---

## 6.1 Centralize file validation in `FileService`

File-size, extension, and MIME-type checks are copy-pasted identically in `PostService` and `UserService`.

| File | Change |
|------|--------|
| [IFileService.cs](file:///c:/Practice/FoodConnectAPI/FoodConnectAPI/Interfaces/Services/IFileService.cs) | Add `void ValidateImageFile(IFormFile file)` — throws `InvalidOperationException` on failure |
| [FileService.cs](file:///c:/Practice/FoodConnectAPI/FoodConnectAPI/Services/FileService.cs) | Implement `ValidateImageFile` with the shared rules |
| [PostService.cs](file:///c:/Practice/FoodConnectAPI/FoodConnectAPI/Services/PostService.cs) | Replace inline validation blocks with `_fileService.ValidateImageFile(file)` |
| [UserService.cs](file:///c:/Practice/FoodConnectAPI/FoodConnectAPI/Services/UserService.cs) | Same replacement |

---

## 6.2 Enable HTTPS redirect

`app.UseHttpsRedirection()` is commented out.

| File | Change |
|------|--------|
| [Program.cs](file:///c:/Practice/FoodConnectAPI/FoodConnectAPI/Program.cs) | Uncomment `app.UseHttpsRedirection()` |

> [!NOTE]
> Only applies when a TLS certificate is configured. Skip if this is still purely local development and note it in `README.md`.

---

## 6.3 Make the rate limiter actually take effect

`UseRateLimiter()` is in the pipeline but the `"fixed"` named policy is never applied as a default, so every request bypasses it.

| File | Change |
|------|--------|
| [Program.cs](file:///c:/Practice/FoodConnectAPI/FoodConnectAPI/Program.cs) | Set `options.GlobalLimiter` using `PartitionedRateLimiter`, **or** add `[EnableRateLimiting("fixed")]` to each controller class |

---

---

# New Files

| File | Location |
|------|----------|
| `FollowService.cs` | Services/ |
| `FollowsController.cs` | Controllers/ |
| `FollowUserDto.cs` | Models/ |
| `ReportService.cs` | Services/ |
| `ReportsController.cs` | Controllers/ |
| `ReportAddDto.cs` | Models/ |
| `ReportInfoDto.cs` | Models/ |
| `UserProfileDto.cs` | Models/ |
| EF migration | Migrations/ — `AddReasonToReport` |

---

# Verification Plan

### Automated Tests
```
dotnet test
```
- Add unit tests for `FollowService` (mirror `LikeServiceTest.cs` pattern).
- Add unit tests for `ReportService`.
- Add comment ownership tests (controller-level or integration).

### Manual Smoke Tests
| Scenario | Expected |
|----------|----------|
| `PATCH /api/comments/{id}` — no token | 401 |
| `PATCH /api/comments/{id}` — wrong user's token | 403 |
| `POST /api/users/{id}/follow` twice | 2nd call returns conflict/no-op |
| `POST /api/users/{id}/follow` with own ID | 400 Bad Request |
| `POST /api/posts/{id}/report` — duplicate | conflict |
| `GET /api/admin/reports` — non-admin token | 403 |
| Update profile picture → check old file deleted | old file gone from `wwwroot/Uploads` |
| `PATCH /api/users/update` — no password field | 200, other fields updated |
