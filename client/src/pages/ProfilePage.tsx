import { useNavigate } from "react-router-dom";
import { logoutRequest } from "../api/auth";
import { useMe } from "../hooks/useMe";
import { useAuthStore } from "../store/authStore";

function formatJoinDate(dateIso: string): string {
  return new Date(dateIso).toLocaleDateString("en-US", {
    year: "numeric",
    month: "long",
    day: "numeric",
  });
}

export default function ProfilePage() {
  const navigate = useNavigate();
  const refreshToken = useAuthStore((state) => state.refreshToken);
  const clearAuth = useAuthStore((state) => state.clearAuth);

  const { data: profile, isLoading, isError } = useMe();

  const handleLogout = async (): Promise<void> => {
    try {
      if (refreshToken) {
        await logoutRequest(refreshToken);
      }
    } catch {
      // ignore — local session is cleared regardless of server response
    } finally {
      clearAuth();
      navigate("/login", { replace: true });
    }
  };

  return (
    <div className="profile-page">
      <button type="button" className="back-button" onClick={() => navigate("/dashboard")}>
        &larr; Back to dashboard
      </button>

      <h1>Profile</h1>

      {isLoading && <p className="page-status">Loading profile...</p>}
      {isError && <p className="page-status page-error">Failed to load profile.</p>}

      {profile && (
        <div className="profile-card">
          <div className="profile-row">
            <span className="profile-label">Email</span>
            <span>{profile.email}</span>
          </div>
          <div className="profile-row">
            <span className="profile-label">Member since</span>
            <span>{formatJoinDate(profile.createdAt)}</span>
          </div>
          <div className="profile-row">
            <span className="profile-label">Habits</span>
            <span>{profile.habitCount}</span>
          </div>
        </div>
      )}

      <button type="button" className="logout-button" onClick={handleLogout}>
        Logout
      </button>
    </div>
  );
}
