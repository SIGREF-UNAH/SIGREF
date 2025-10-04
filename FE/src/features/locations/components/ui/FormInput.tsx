interface FormInputProps {
  label: string;
  value: string;
  onChange: (v: string) => void;
  placeholder?: string;
  type?: string;
  className?: string;
}

export default function FormInput({
  label,
  value,
  onChange,
  placeholder,
  type = "text",
  className = "",
}: FormInputProps) {
  return (
    <div className={className}>
      <label className="block text-base font-medium text-[#616161] mb-2">
        {label}
      </label>
      <input
        type={type}
        placeholder={placeholder}
        value={value}
        onChange={(e) => onChange(e.target.value)}
        className="w-full px-3 py-2 bg-[#D9D9D9] font-normal border border-gray-200 rounded-md text-[#616161] placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
      />
    </div>
  );
}
