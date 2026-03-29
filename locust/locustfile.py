from locust import HttpUser, task, between
import random, string

class PhotoAppUser(HttpUser):
    wait_time = between(1, 3)
    gallery_id = None

    def on_start(self):
        self.prefix = ''.join(random.choices(string.ascii_lowercase + string.digits, k=8))
        self.email, self.password = f"{self.prefix}@test.com", f"Pass_{self.prefix}"
        
        self.client.post("/register", json={"email": self.email, "password": self.password}, name="/register")
        res = self.client.post("/login", json={"email": self.email, "password": self.password}, name="/login")
        if res.status_code == 200:
            self.client.headers.update({"Authorization": f"Bearer {res.json().get('accessToken')}"})

    @task(1)
    def ensure_gallery(self):
        if self.gallery_id is None:
            res = self.client.post("/api/Galleries", json={"name": f"Gal_{self.prefix}", "isPublic": False}, name="/api/Galleries")
            if res.status_code == 200:
                gals = self.client.get("/api/Galleries?mine=true", name="/api/Galleries").json()
                if gals: self.gallery_id = gals[0].get("id")

    @task(10)
    def list_photos(self):
        if self.gallery_id:
            self.client.get(f"/api/photos?galleryId={self.gallery_id}", name="GET /api/photos")

    @task(2)
    def upload_photo(self):
        if self.gallery_id:
            files = {"File": ("test.jpg", b"fake_data", "image/jpeg")}
            self.client.post(f"/api/photos?galleryId={self.gallery_id}", data={"Name": "img"}, files=files, name="POST /api/photos")

    @task(1)
    def delete_everything(self):
        if not self.gallery_id: return
        
        self.client.delete(f"/api/Galleries/{self.gallery_id}", name="DELETE /api/Galleries/{id}")
        self.gallery_id = None