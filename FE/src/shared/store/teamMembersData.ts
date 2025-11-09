export interface TeamMember {
  id: number;
  name: string;
  role: string;
  phone: string;
  email: string;
  github?: {
    username: string;
  }
}

export interface TeamMemberWithAvatar extends TeamMember {
  avatarUrl?: string;
}

export const teamMembers: TeamMember[] = [
  {
    id: 1,
    name: "Anthony Miranda",
    role: "Backend & Frontend",
    phone: "+504 9735 4899",
    email: "anthony07miranda@gmail.com",
    github: {
      username: "AnthonyEMF",
    }
  },
  {
    id: 2,
    name: "Carlos Pineda",
    role: "Backend & Frontend",
    phone: "+504 9928 4952",
    email: "carlosovidiopineda8@gmail.com",
    github: {
      username: "Pineda04",
    }
  },
  {
    id: 3,
    name: "Danilo Vides",
    role: "Frontend",
    phone: "+504 3359 4917",
    email: "vch.daniloisaac@gmail.com",
    github: {
      username: "IsaacV04",
    }
  },
  {
    id: 4,
    name: "David Díaz",
    role: "Backend",
    phone: "+504 9688 6567",
    email: "david1970josue@gmail.com",
    github: {
      username: "JDDR200530",
    }
  },
  {
    id: 5,
    name: "Ever García",
    role: "Frontend",
    phone: "+504 8818 4760",
    email: "elever744@gmail.com",
    github: {
      username: "everjosue56",
    }
  },
  {
    id: 6,
    name: "Erick Arita",
    role: "Backend",
    phone: "-",
    email: "-",
    github: {
      username: "erickArita",
    }
  },
  {
    id: 7,
    name: "Hector Martinez",
    role: "Backend",
    phone: "+504 9750 6626",
    email: "hectormartinez1vg@gmail.com",
    github: {
      username: "TETvega",
    }
  },
  {
    id: 8,
    name: "Michael Galdamez",
    role: "Frontend",
    phone: "+504 3260 0891",
    email: "michaelcom292@gmail.com",
    github: {
      username: "MichaelGald",
    }
  },
];
