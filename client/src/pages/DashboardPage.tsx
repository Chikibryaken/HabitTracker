import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { logoutRequest } from "../api/auth";
import Modal from "../components/Modal";
import HabitForm from "../features/habits/HabitForm";
import HabitList from "../features/habits/HabitList";
import { useCreateHabit } from "../features/habits/useHabitMutations";
import { useAuthStore } from "../store/authStore";

export default function DashboardPage() {
  const navigate = useNavigate();
  const user = useAuthStore((state) => state.user);
  const refreshToken = useAuthStore((state) => state.refreshToken);
  const clearAuth = useAuthStore((state) => state.clearAuth);

  const [isCreating, setIsCreating] = useState(false);
  const createHabit = useCreateHabit();

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
    <div className="dashboard-page">
      <header className="dashboard-header">
        <Link to="/profile" className="dashboard-email">
          {user?.email}
        </Link>
        <div className="dashboard-header-actions">
          <button type="button" onClick={() => setIsCreating(true)}>
            New habit
          </button>
          <button type="button" className="logout-button" onClick={handleLogout}>
            Logout
          </button>
        </div>
      </header>

      <HabitList />

      {isCreating && (
        <Modal title="New habit" onClose={() => setIsCreating(false)}>
          <HabitForm
            submitLabel="Create"
            isSubmitting={createHabit.isPending}
            onCancel={() => setIsCreating(false)}
            onSubmit={(input) => {
              createHabit.mutate(input, { onSuccess: () => setIsCreating(false) });
            }}
          />
        </Modal>
      )}
    </div>
  );
}
