from locust import HttpUser, task, between
import random, string

class PhotoGalleryUser(HttpUser):
    wait_time = between(1, 3)
    gallery_id = None

    def on_start(self):
        # Regisztráció és bejelentkezés
        email = f"user_{''.join(random.choices(string.ascii_lowercase, k=6))}@test.com"
        self.client.post("/register", json={"email": email, "password": "Password123!"})
        res = self.client.post("/login", json={"email": email, "password": "Password123!"})
        
        if res.status_code == 200:
            token = res.json().get("accessToken")
            self.client.headers.update({"Authorization": f"Bearer {token}"})
            
            # Galéria létrehozása és ID kinyerése
            self.client.post("/api/Galleries", json={"name": "Teszt Galéria", "isPublic": True})
            galleries = self.client.get("/api/Galleries?mine=true").json()
            if galleries:
                self.gallery_id = galleries[0].get("id")

    @task(3)
    def upload_photo(self):
        if not self.gallery_id: return
        # Fájlfeltöltés szimulálása 
        files = {"File": ("test.jpg", b"dummy_image_bytes", "image/jpeg")}
        data = {"Name": f"Kép {random.randint(1,100)}"}
        self.client.post(f"/api/photos?galleryId={self.gallery_id}", data=data, files=files)

    @task(4)
    def list_and_view(self):
        if not self.gallery_id: return
        # Listázás dátum szerint
        res = self.client.get(f"/api/photos?galleryId={self.gallery_id}&sortBy=date&sortDesc=true")
        if res.status_code == 200 and res.json():
            photos = res.json()
            photo_id = random.choice(photos).get("id")
            # Kép megtekintése
            self.client.get(f"/api/photos/{photo_id}/view?galleryId={self.gallery_id}")

    @task(1)
    def delete_photo(self):
        if not self.gallery_id: return
        res = self.client.get(f"/api/photos?galleryId={self.gallery_id}")
        if res.status_code == 200 and res.json():
            photo_id = res.json()[0].get("id")
            # Kép törlése
            self.client.delete(f"/api/photos/{photo_id}?galleryId={self.gallery_id}")