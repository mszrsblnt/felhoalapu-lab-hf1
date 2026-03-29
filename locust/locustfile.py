import os
import random
import string
from locust import HttpUser, task, between

class PhotoAppUser(HttpUser):
    wait_time = between(1, 3)
    known_photo_ids = []
    gallery_id = None

    def on_start(self):
        uid = ''.join(random.choices(string.ascii_lowercase + string.digits, k=6))
        email, password = f"user_{uid}@test.com", "Password123!"
        
        self.client.post("/register", json={"email": email, "password": password}, name="/register")
        res = self.client.post("/login", json={"email": email, "password": password}, name="/login")
        
        if res.status_code == 200:
            token = res.json().get("accessToken")
            self.client.headers.update({"Authorization": f"Bearer {token}"})
            
            self.client.post("/api/Galleries", json={"name": f"Gal_{uid}", "isPublic": True}, name="/api/Galleries")
            gals = self.client.get("/api/Galleries?mine=true", name="/api/Galleries?mine=true").json()
            if gals:
                self.gallery_id = gals[0].get("id")

    @task(4)
    def get_photos_list(self):
        if not self.gallery_id: return
        with self.client.get(f"/api/photos?galleryId={self.gallery_id}&sortBy=date&sortDesc=true", name="GET /api/photos", catch_response=True) as res:
            if res.status_code == 200:
                photos = res.json()
                if photos:
                    self.known_photo_ids = [p['id'] for p in photos]
            elif res.status_code == 401:
                res.success() 

    @task(3)
    def get_single_photo_view(self):
        if self.known_photo_ids and self.gallery_id:
            p_id = random.choice(self.known_photo_ids)
            with self.client.get(f"/api/photos/{p_id}/view?galleryId={self.gallery_id}", name="GET /api/photos/{id}/view", catch_response=True) as res:
                if res.status_code in [200, 404, 401]:
                    res.success()

    @task(1)
    def photo_lifecycle(self):
        if not self.gallery_id: return
        
        files = {'File': ('test.jpg', b'fake_data', 'image/jpeg')}
        data = {'Name': f'Photo_{random.randint(1,999)}'}
        
        with self.client.post(f"/api/photos?galleryId={self.gallery_id}", data=data, files=files, name="POST /api/photos", catch_response=True) as res:
            if res.status_code == 200:
                res.success()
                p_id = res.json().get('id') if isinstance(res.json(), dict) else None
                if p_id:
                    self.client.delete(f"/api/photos/{p_id}?galleryId={self.gallery_id}", name="DELETE /api/photos/{id}")
            elif res.status_code == 401:
                res.success()