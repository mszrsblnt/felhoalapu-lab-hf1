terraform {
  required_providers {
    google = {
      source  = "hashicorp/google"
      version = "~> 5.0"
    }
  }
  backend "gcs" {
    bucket = "terraform-state-cloud-lab-mszrsblnt"
    prefix = "terraform/state"
  }
}

provider "google" {
  project = var.project_id
  region  = "europe-west1"
}

variable "project_id" { type = string }

# Google Cloud projekt adatainak lekérése
data "google_project" "project" {
  project_id = var.project_id
}

# URL-ek kiszámolása
locals {
  backend_url  = "https://photo-gallery-backend-${data.google_project.project.number}.europe-west1.run.app"
  frontend_url = "https://photo-gallery-frontend-${data.google_project.project.number}.europe-west1.run.app"
}

# --- 1. ADATBÁZIS ---
resource "google_sql_database_instance" "postgres" {
  name             = "cloud-lab-db"
  database_version = "POSTGRES_15"
  region           = "europe-west1"
  settings {
    tier = "db-f1-micro"
    deletion_protection_enabled = true # Védelem a véletlen törlés ellen
  }
}

resource "google_sql_database" "database" {
  name     = "photogallery"
  instance = google_sql_database_instance.postgres.name
}

resource "google_sql_user" "db_user" {
  name     = "dbuser"
  instance = google_sql_database_instance.postgres.name
  password = "Password123"
}

# --- 2. BACKEND ---
resource "google_cloud_run_v2_service" "backend" {
  name     = "photo-gallery-backend"
  location = "europe-west1"

  template {
    scaling { max_instance_count = 5 }
    
    containers {
      image = "europe-west1-docker.pkg.dev/${var.project_id}/cloud-run-source-deploy/felhoalapu-lab-hf1/cloud-lab-be:latest"
      
      volume_mounts {
        name       = "cloudsql"
        mount_path = "/cloudsql"
      }
      
      env {
        name  = "ConnectionStrings__DefaultConnection"
        value = "Host=/cloudsql/${var.project_id}:europe-west1:${google_sql_database_instance.postgres.name};Database=${google_sql_database.database.name};Username=${google_sql_user.db_user.name};Password=${google_sql_user.db_user.password}"
      }
      env {
        name  = "FrontendUrl"
        value = local.frontend_url
      }
    }

    volumes {
      name = "cloudsql"
      cloud_sql_instance {
        instances = [google_sql_database_instance.postgres.connection_name]
      }
    }
  }
}

# --- 3. FRONTEND ---
resource "google_cloud_run_v2_service" "frontend" {
  name     = "photo-gallery-frontend"
  location = "europe-west1"

  template {
    containers {
      image = "europe-west1-docker.pkg.dev/${var.project_id}/cloud-run-source-deploy/felhoalapu-lab-hf1/cloud-lab-fe:latest"
      
      env {
        name  = "API_URL"
        value = local.backend_url
      }
    }
  }
}

# --- 4. IAM ÉS TŰZFAL ---
resource "google_cloud_run_service_iam_member" "noauth_backend" {
  location = google_cloud_run_v2_service.backend.location
  service  = google_cloud_run_v2_service.backend.name
  role     = "roles/run.invoker"
  member   = "allUsers"
}

resource "google_cloud_run_service_iam_member" "noauth_frontend" {
  location = google_cloud_run_v2_service.frontend.location
  service  = google_cloud_run_v2_service.frontend.name
  role     = "roles/run.invoker"
  member   = "allUsers"
}

