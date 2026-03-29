from locust import HttpUser, task, between
import random, string

class PhotoGalleryUser(HttpUser):
    wait_time = between(1, 4)
    gallery_id = None

    def on_start(self):
        uid = ''.join(random.choices(string.ascii_lowercase + string.digits, k=8))
        self.email, self.password = f"user_{uid}@test.com", "Password123!"
        
        self.client.post("/register", json={"email": self.email, "password": self.password}, name="/register")
        res = self.client.post("/login", json={"email": self.email, "password": self.password}, name="/login")
        
        if res.status_code == 200:
            self.client.headers.update({"Authorization": f"Bearer {res.json().get('accessToken')}"})
            self.client.post("/api/Galleries", json={"name": f"Gal_{uid}", "isPublic": False}, name="/api/Galleries")
            gals = self.client.get("/api/Galleries?mine=true", name="/api/Galleries").json()
            if gals: self.gallery_id = gals[0].get("id")

    @task(10)
    def main_flow(self):
        if not self.gallery_id: return
        
        files = {"File": ("test.jpg", b"fake_data", "image/jpeg")}
        data = {"Name": f"Img_{random.randint(1, 9999)}"}
        
        with self.client.post(f"/api/photos?galleryId={self.gallery_id}", data=data, files=files, name="POST /api/photos", catch_response=True) as res:
            if res.status_code in [200, 401]:
                res.success()
                if res.status_code == 200:
                    list_res = self.client.get(f"/api/photos?galleryId={self.gallery_id}", name="GET /api/photos")
                    if list_res.status_code == 200 and list_res.json():
                        p_id = list_res.json()[-1].get("id")
                        self.client.get(f"/api/photos/{p_id}/view?galleryId={self.gallery_id}", name="GET /api/photos/{id}/view")

    @task(1)
    def cleanup(self):
        if not self.gallery_id: return
        res = self.client.get(f"/api/photos?galleryId={self.gallery_id}", name="GET /api/photos")
        if res.status_code == 200 and res.json():
            p_id = res.json()[0].get("id")
            with self.client.delete(f"/api/photos/{p_id}?galleryId={self.gallery_id}", name="DELETE /api/photos/{id}", catch_response=True) as d_res:
                if d_res.status_code in [200, 204, 404]: d_res.success()