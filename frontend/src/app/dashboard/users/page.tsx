"use client";

import { useState, useEffect } from "react";
import { useAuth } from "@/lib/auth";
import { usersApi } from "@/lib/api";
import { RoleBadge } from "@/components/ui/RoleBadge";
import { Modal } from "@/components/ui/Modal";
import { MOCK_USERS as MOCK_USERS_DATA } from "@/lib/mock-data";
import type { User, UserRole, UserCreatePayload } from "@/lib/types";
import { ROLE_LABELS } from "@/lib/types";

const ALL_ROLES: UserRole[] = ["ADMIN", "SALES", "PURCHASING", "WAREHOUSE", "ROUTE"];

export default function UsersPage() {
  const { hasRole, isMockMode } = useAuth();
  const [users, setUsers] = useState<User[]>(isMockMode ? MOCK_USERS_DATA : []);
  const [loading, setLoading] = useState(!isMockMode);
  const [showModal, setShowModal] = useState(false);
  const [editingUser, setEditingUser] = useState<User | null>(null);
  const [form, setForm] = useState<UserCreatePayload>({ username: "", password: "", role: "SALES" });

  useEffect(() => {
    if (!isMockMode) {
      usersApi.list().then(setUsers).catch(console.error).finally(() => setLoading(false));
    }
  }, [isMockMode]);

  if (!hasRole("ADMIN")) {
    return (
      <div className="flex items-center justify-center py-20">
        <div className="panel p-10 text-center max-w-sm">
          <p className="text-text-secondary text-sm">Access denied. Admin only.</p>
        </div>
      </div>
    );
  }

  function openCreate() {
    setEditingUser(null);
    setForm({ username: "", password: "", role: "SALES" });
    setShowModal(true);
  }

  function openEdit(user: User) {
    setEditingUser(user);
    setForm({ username: user.username, password: "", role: user.role });
    setShowModal(true);
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (isMockMode) {
      if (editingUser) {
        setUsers((prev) => prev.map((u) => (u.id === editingUser.id ? { ...u, username: form.username, role: form.role } : u)));
      } else {
        setUsers((prev) => [...prev, { id: Date.now(), username: form.username, role: form.role }]);
      }
    } else {
      if (editingUser) {
        const updated = await usersApi.update(editingUser.id, { username: form.username, role: form.role, ...(form.password ? { password: form.password } : {}) });
        setUsers((prev) => prev.map((u) => (u.id === editingUser.id ? updated : u)));
      } else {
        const created = await usersApi.create(form);
        setUsers((prev) => [...prev, created]);
      }
    }
    setShowModal(false);
  }

  async function handleDelete(id: number) {
    if (!confirm("Are you sure you want to delete this user?")) return;
    if (isMockMode) {
      setUsers((prev) => prev.filter((u) => u.id !== id));
    } else {
      await usersApi.delete(id);
      setUsers((prev) => prev.filter((u) => u.id !== id));
    }
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-semibold text-text-primary">User Management</h1>
          <p className="text-[13px] text-text-muted mt-1">{users.length} registered users</p>
        </div>
        <button onClick={openCreate} className="btn-primary">
          <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
            <path strokeLinecap="round" strokeLinejoin="round" d="M12 4.5v15m7.5-7.5h-15" />
          </svg>
          Add User
        </button>
      </div>

      {loading ? (
        <div className="flex items-center justify-center py-24">
          <div className="w-8 h-8 rounded-full border-2 border-accent-500 border-t-transparent animate-spin" />
        </div>
      ) : (
        <div className="panel overflow-hidden">
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr>
                  <th className="table-header">ID</th>
                  <th className="table-header">Username</th>
                  <th className="table-header">Role</th>
                  <th className="table-header text-right">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-[rgba(255,255,255,0.05)]">
                {users.map((user) => (
                  <tr key={user.id} className="table-row">
                    <td className="table-cell font-mono text-text-muted text-[12px]">{user.id}</td>
                    <td className="table-cell text-[14px] text-text-primary font-medium">{user.username}</td>
                    <td className="table-cell"><RoleBadge role={user.role} /></td>
                    <td className="table-cell text-right">
                      <div className="flex items-center justify-end gap-1">
                        <button onClick={() => openEdit(user)} className="btn-icon" title="Edit">
                          <svg className="w-[18px] h-[18px]" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1.5}>
                            <path strokeLinecap="round" strokeLinejoin="round" d="M16.862 4.487l1.687-1.688a1.875 1.875 0 112.652 2.652L10.582 16.07a4.5 4.5 0 01-1.897 1.13L6 18l.8-2.685a4.5 4.5 0 011.13-1.897l8.932-8.931zm0 0L19.5 7.125M18 14v4.75A2.25 2.25 0 0115.75 21H5.25A2.25 2.25 0 013 18.75V8.25A2.25 2.25 0 015.25 6H10" />
                          </svg>
                        </button>
                        <button onClick={() => handleDelete(user.id)} className="btn-icon btn-icon-danger" title="Delete">
                          <svg className="w-[18px] h-[18px]" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1.5}>
                            <path strokeLinecap="round" strokeLinejoin="round" d="M14.74 9l-.346 9m-4.788 0L9.26 9m9.968-3.21c.342.052.682.107 1.022.166m-1.022-.165L18.16 19.673a2.25 2.25 0 01-2.244 2.077H8.084a2.25 2.25 0 01-2.244-2.077L4.772 5.79m14.456 0a48.108 48.108 0 00-3.478-.397m-12 .562c.34-.059.68-.114 1.022-.165m0 0a48.11 48.11 0 013.478-.397m7.5 0v-.916c0-1.18-.91-2.164-2.09-2.201a51.964 51.964 0 00-3.32 0c-1.18.037-2.09 1.022-2.09 2.201v.916m7.5 0a48.667 48.667 0 00-7.5 0" />
                          </svg>
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      <Modal isOpen={showModal} onClose={() => setShowModal(false)} title={editingUser ? "Edit User" : "Create User"}>
        <form onSubmit={handleSubmit} className="space-y-5">
          <div>
            <label htmlFor="modal-username" className="field-label">Username</label>
            <input id="modal-username" type="text" required value={form.username}
              onChange={(e) => setForm((f) => ({ ...f, username: e.target.value }))} className="input-field" />
          </div>
          <div>
            <label htmlFor="modal-password" className="field-label">
              Password {editingUser && <span className="text-text-muted font-normal">(leave blank to keep)</span>}
            </label>
            <input id="modal-password" type="password" required={!editingUser} value={form.password}
              onChange={(e) => setForm((f) => ({ ...f, password: e.target.value }))} className="input-field" />
          </div>
          <div>
            <label htmlFor="modal-role" className="field-label">Role</label>
            <select id="modal-role" value={form.role}
              onChange={(e) => setForm((f) => ({ ...f, role: e.target.value as UserRole }))} className="input-field">
              {ALL_ROLES.map((r) => (
                <option key={r} value={r}>{ROLE_LABELS[r]}</option>
              ))}
            </select>
          </div>
          <div className="flex justify-end gap-3 pt-3 border-t border-[rgba(255,255,255,0.08)]">
            <button type="button" onClick={() => setShowModal(false)} className="btn-secondary">Cancel</button>
            <button type="submit" className="btn-primary">{editingUser ? "Save Changes" : "Create User"}</button>
          </div>
        </form>
      </Modal>
    </div>
  );
}
