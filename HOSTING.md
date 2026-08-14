# Hosting SkillGraph (free)

## 1. Push these files to GitHub

```bash
git add backend/Dockerfile backend/.dockerignore frontend/vercel.json frontend/src/environments frontend/src/app/api.service.ts frontend/angular.json render.yaml HOSTING.md
git commit -m "Add free hosting config (Render + Vercel)"
git push
```

## 2. Backend on Render (free)

1. https://render.com → Sign up with GitHub  
2. **New +** → **Web Service** → select repo `SkillGraph`  
3. Settings:
   - **Name:** skillgraph-api  
   - **Root Directory:** `backend`  
   - **Runtime:** Docker  
   - **Dockerfile Path:** `./Dockerfile`  
   - **Instance type:** Free  
4. Environment variables:

| Key | Value |
|-----|--------|
| `COGNODB_URI` | `bolt+s://db-dc597258.databases.cognodb.com` |
| `COGNODB_USER` | `cognodb` |
| `COGNODB_PASSWORD` | *(your password)* |
| `COGNODB_DATABASE` | `neo4j` |
| `ASPNETCORE_ENVIRONMENT` | `Production` |

5. Create Web Service → wait for deploy  
6. Copy URL, e.g. `https://skillgraph-api-xxxx.onrender.com`  
7. Test: `https://YOUR-URL.onrender.com/api/graph/health`

## 3. Point Vercel rewrites at Render

Edit `frontend/vercel.json` and replace:

```text
https://REPLACE_WITH_RENDER_URL.onrender.com
```

with your real Render URL (no trailing slash), e.g.:

```json
"destination": "https://skillgraph-api-xxxx.onrender.com/api/:path*"
```

Commit and push.

## 4. Frontend on Vercel (free)

1. https://vercel.com → Sign up with GitHub  
2. **Add New Project** → import `SkillGraph`  
3. Settings:
   - **Root Directory:** `frontend`  
   - **Framework Preset:** Other (or Angular)  
   - **Build Command:** `npm run build`  
   - **Output Directory:** `dist/frontend/browser`  
4. Deploy  
5. Open the Vercel URL → Load sample data → explore  

## 5. Submit

Email hr@wexa.ai with:
- GitHub: https://github.com/JPHemanthKumaar/SkillGraph  
- Live demo: https://your-app.vercel.app  
- Screen recording link  
