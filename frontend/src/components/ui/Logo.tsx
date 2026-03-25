export function Logo({ className = "w-8 h-8" }: { className?: string }) {
  return (
    <svg
      className={className}
      viewBox="0 0 32 32"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
    >
      <path
        d="M8 6C8 4.89543 8.89543 4 10 4V4C11.1046 4 12 4.89543 12 6V26C12 27.1046 11.1046 28 10 28V28C8.89543 28 8 27.1046 8 26V6Z"
        fill="currentColor"
      />
      <path
        d="M20 6C20 4.89543 20.8954 4 22 4V4C23.1046 4 24 4.89543 24 6V26C24 27.1046 23.1046 28 22 28V28C20.8954 28 20 27.1046 20 26V6Z"
        fill="currentColor"
      />
      <path
        d="M12 18L20 14"
        stroke="var(--color-accent-500)"
        strokeWidth="4"
        strokeLinecap="round"
      />
    </svg>
  );
}
