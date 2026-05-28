# Real-time Dating App

This is a full-stack dating app I built mainly to push my Angular further and to learn SignalR, specifically real-time messaging and presence tracking, which were the two things I had not done before. The backend is a .NET Web API, the front end is an Angular SPA, and the two talk over both HTTP and SignalR for the live features.

> No live demo. It was hosted on Azure App Service during development, but the free trial expired and keeping a full .NET plus MSSQL app running costs more than a portfolio piece justifies. The screenshots below cover the actual functionality.

## What I was actually learning

The brief I set myself was real-time behaviour, so that is where the interesting work is. Messages arrive through a SignalR hub and appear without a refresh, and member cards show live online/offline state through a presence hub that tracks connections as they open and close. Getting presence right was the harder of the two, because "online" is not a stored field, it is a function of which connections are currently live, and that has to survive reconnects and multiple tabs without lying about who is actually there.

The rest (role-based auth with Identity, the likes system, profile and photo management, an admin panel for roles) is more conventional CRUD, built to give the real-time features something real to sit on top of rather than as the focus.

## On JWT authentication

This project uses JWT for authentication because the course required it. Having looked into it properly since, I think it was the wrong call for an app like this, and I will not repeat it. I am leaving the reasoning here because the decision is in the code and I would rather explain what I would change than pretend I would not change anything.

The problem is where the token has to live, and both options are bad.

Store it in a cookie and you have reinvented the session cookie, except the token is far larger, carries the user's data on every request, and is much harder to revoke. You have added complexity and bandwidth and gained nothing over a plain session.

Store it in localStorage and you have opened the door to XSS. localStorage is a plain JavaScript API, so any compromised third-party script on the page (a CDN, an analytics tag, a tracking pixel) can read it and exfiltrate the token. OWASP is explicit that sensitive data should not live there.

Revocation is the other half. Without a server-side blocklist, forcibly logging someone out means rotating the signing key, which logs everyone out. Add a blocklist to fix that and you are back to a database lookup on every request, which defeats the performance argument that was the reason to reach for JWTs in the first place.

JWTs make sense for short-lived, single-use flows: a password reset link, an external login handoff between domains, anything consumed once and discarded. As a persistent session mechanism for a web app, they are a misuse of the format. Future projects use session cookies.

[The video where this clicked for me](https://www.youtube.com/watch?v=JdGOb7AxUo0&t=12s)

## Tech

Backend: .NET Web API, EF Core, MSSQL, ASP.NET Identity (User / Moderator / Admin roles), and SignalR for messaging and presence. Front end: Angular with Tailwind CSS. It was deployed to Azure App Service while the trial lasted.

## What it does

Members can browse everyone with filtering by gender and minimum age, with a live presence indicator on each card, and open detailed profiles with photo galleries. The likes system has three views: who you like, who liked you, and mutual matches. Messaging is a full inbox over SignalR with per-member conversation history. Profile management covers editing details, uploading and managing photos, and setting a main photo. The admin panel handles viewing users and assigning or removing Moderator and Admin roles.

## Known limitations

Desktop only and the mobile layout is not done. Photo upload is drag-and-drop only so far (click-to-upload is on the list). The registration form's validation error styling needs polish, and the photo management UI has room to improve. Tested and working on Chrome/Brave and Firefox/Zen.

## Screenshots

### Browse: unfiltered
![All members browse view](images/readme/screen1.png)
![All members browse view 2](images/readme/screen2.png)

### Browse: filtered
![Female only, minimum age 40](images/readme/female%20only%20min%20age%2040.png)
![Male only](images/readme/male-only.png)

### Likes: who you like
![Members you have liked](images/readme/liked-example.png)

### Likes: who liked you
![Members who liked you](images/readme/liked-me.png)

### Likes: mutual matches
![Mutual matches](images/readme/mutual.png)

### Messaging
![Message conversation example](images/readme/message-example.png)
![Message conversation example 2](images/readme/message-example2.png)

### Member profile
![Member profile view](images/readme/profile.png)

### Profile: edit
![Edit profile details](images/readme/profile-edit.png)

### Profile: photos
![Profile photo management](images/readme/profile-photos.png)

### Admin: user management
![Admin user management panel](images/readme/admin%20user%20management.png)

### Admin: role editing
![Admin role editing panel](images/readme/admin%20role%20editing.png)
