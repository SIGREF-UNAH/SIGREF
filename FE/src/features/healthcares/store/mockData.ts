export interface Healthcare {
  id: string;
  active: boolean;
  name: string;
  abbreviation: string;
  comment: string | null;
  cost: number;
  specialty: Array<{ text: string }>;
  providedBy: {
    reference: string;
    display: string;
  } | null;
  location: Array<{ reference: string; display: string }> | null;
}

export const mockData: Healthcare[] = [
  {
    id: "1",
    active: true,
    name: "Examen de Sangre Completo",
    abbreviation: "EXS",
    comment: "Perfil lipídico, glucosa, hemograma completo",
    cost: 100,
    specialty: [{ text: "Medicina General" }],
    providedBy: {
      reference: "Organization/1",
      display: "Hospital de Occidente"
    },
    location: [{ reference: "Location/1", display: "Laboratorio Central" }]
  },
  {
    id: "2",
    active: true,
    name: "Radiografía de Tórax",
    abbreviation: "RX-TX",
    comment: "Proyección posteroanterior y lateral",
    cost: 150,
    specialty: [{ text: "Radiología" }],
    providedBy: {
      reference: "Organization/2",
      display: "Clínica de Occidente"
    },
    location: [{ reference: "Location/2", display: "Sala de Rayos X" }]
  },
  {
    id: "3",
    active: true,
    name: "Cirugía de Riñón",
    abbreviation: "CR",
    comment: "Nefrectomía parcial por tumor",
    cost: 8500,
    specialty: [{ text: "Urología" }, { text: "Cirugía General" }],
    providedBy: null,
    location: null
  },
  {
    id: "4",
    active: true,
    name: "Resonancia Magnética Cerebral",
    abbreviation: "RM-CEREBRAL",
    comment: "Con contraste gadolinio",
    cost: 650,
    specialty: [{ text: "Radiología" }, { text: "Neurología" }],
    providedBy: {
      reference: "Organization/3",
      display: "Centro Médico Metropolitano"
    },
    location: [{ reference: "Location/3", display: "Unidad de Imagenología" }]
  },
  {
    id: "5",
    active: true,
    name: "Consulta Cardiológica",
    abbreviation: "CON-CARDIO",
    comment: "Evaluación cardiovascular completa",
    cost: 120,
    specialty: [{ text: "Cardiología" }],
    providedBy: {
      reference: "Organization/4",
      display: "Instituto Cardiológico"
    },
    location: [{ reference: "Location/4", display: "Consulta Externa" }]
  },
  {
    id: "6",
    active: false,
    name: "Tomografía Abdominal",
    abbreviation: "TAC-ABD",
    comment: "Protocolo para pancreatitis aguda",
    cost: 420,
    specialty: [{ text: "Radiología" }],
    providedBy: {
      reference: "Organization/1",
      display: "Hospital de Occidente"
    },
    location: [{ reference: "Location/5", display: "Tomógrafo 2" }]
  },
  {
    id: "7",
    active: true,
    name: "Ecografía Obstétrica",
    abbreviation: "ECO-OBST",
    comment: "Control trimestral del embarazo",
    cost: 95,
    specialty: [{ text: "Ginecología" }, { text: "Radiología" }],
    providedBy: {
      reference: "Organization/5",
      display: "Maternidad Santa Ana"
    },
    location: [{ reference: "Location/6", display: "Ecografía 3" }]
  },
  {
    id: "8",
    active: true,
    name: "Endoscopia Digestiva Alta",
    abbreviation: "EDA",
    comment: "Con biopsia si se requiere",
    cost: 380,
    specialty: [{ text: "Gastroenterología" }],
    providedBy: {
      reference: "Organization/6",
      display: "Centro Gastrointestinal"
    },
    location: [{ reference: "Location/7", display: "Sala de Endoscopias" }]
  },
  {
    id: "9",
    active: true,
    name: "Prueba de Esfuerzo",
    abbreviation: "PE",
    comment: "Protocolo de Bruce en banda sin fin",
    cost: 180,
    specialty: [{ text: "Cardiología" }],
    providedBy: {
      reference: "Organization/4",
      display: "Instituto Cardiológico"
    },
    location: [{ reference: "Location/8", display: "Laboratorio de Pruebas" }]
  },
  {
    id: "10",
    active: true,
    name: "Consulta Dermatológica",
    abbreviation: "CON-DERMA",
    comment: "Evaluación de lesiones cutáneas",
    cost: 85,
    specialty: [{ text: "Dermatología" }],
    providedBy: {
      reference: "Organization/7",
      display: "Clínica de la Piel"
    },
    location: [{ reference: "Location/9", display: "Consulta 5" }]
  },
  {
    id: "11",
    active: true,
    name: "Cirugía de Cataratas",
    abbreviation: "CC",
    comment: "Facomulsificación con lente intraocular",
    cost: 2200,
    specialty: [{ text: "Oftalmología" }],
    providedBy: {
      reference: "Organization/8",
      display: "Centro Oftalmológico Avanzado"
    },
    location: [{ reference: "Location/10", display: "Quirófano Oftalmológico" }]
  },
  {
    id: "12",
    active: true,
    name: "Análisis de Orina Completo",
    abbreviation: "UROANALISIS",
    comment: "Incluye urocultivo y antibiograma",
    cost: 45,
    specialty: [{ text: "Laboratorio Clínico" }],
    providedBy: {
      reference: "Organization/1",
      display: "Hospital de Occidente"
    },
    location: [{ reference: "Location/1", display: "Laboratorio Central" }]
  },
  {
    id: "13",
    active: true,
    name: "Mamografía Bilateral",
    abbreviation: "MAMO",
    comment: "Screening para cáncer de mama",
    cost: 130,
    specialty: [{ text: "Radiología" }, { text: "Mastología" }],
    providedBy: {
      reference: "Organization/9",
      display: "Centro de la Mujer"
    },
    location: [{ reference: "Location/11", display: "Unidad de Mamografía" }]
  },
  {
    id: "14",
    active: true,
    name: "Fisioterapia Respiratoria",
    abbreviation: "FTR-RESP",
    comment: "Sesión de 45 minutos",
    cost: 60,
    specialty: [{ text: "Fisioterapia" }, { text: "Neumología" }],
    providedBy: {
      reference: "Organization/10",
      display: "Centro de Rehabilitación"
    },
    location: [{ reference: "Location/12", display: "Sala de Fisioterapia" }]
  },
  {
    id: "15",
    active: false,
    name: "Colonoscopia Total",
    abbreviation: "COLONO",
    comment: "Con sedación consciente",
    cost: 520,
    specialty: [{ text: "Gastroenterología" }],
    providedBy: {
      reference: "Organization/6",
      display: "Centro Gastrointestinal"
    },
    location: [{ reference: "Location/7", display: "Sala de Endoscopias" }]
  },
  {
    id: "16",
    active: true,
    name: "Consulta Psiquiátrica",
    abbreviation: "CON-PSIQ",
    comment: "Evaluación inicial 60 minutos",
    cost: 150,
    specialty: [{ text: "Psiquiatría" }],
    providedBy: {
      reference: "Organization/11",
      display: "Instituto de Salud Mental"
    },
    location: [{ reference: "Location/13", display: "Consulta Psiquiatría" }]
  },
  {
    id: "17",
    active: true,
    name: "Ecocardiograma Doppler",
    abbreviation: "ECO-DOPPLER",
    comment: "Evaluación de válvulas cardíacas",
    cost: 240,
    specialty: [{ text: "Cardiología" }],
    providedBy: {
      reference: "Organization/4",
      display: "Instituto Cardiológico"
    },
    location: [{ reference: "Location/14", display: "Ecocardiografía" }]
  },
  {
    id: "18",
    active: true,
    name: "Prueba de Alergias",
    abbreviation: "TEST-ALERG",
    comment: "Panel de 40 alérgenos comunes",
    cost: 190,
    specialty: [{ text: "Alergología" }, { text: "Inmunología" }],
    providedBy: {
      reference: "Organization/12",
      display: "Centro Alergológico"
    },
    location: [{ reference: "Location/15", display: "Laboratorio de Alergias" }]
  },
  {
    id: "19",
    active: true,
    name: "Cirugía de Apéndice",
    abbreviation: "APENDICECTOMIA",
    comment: "Procedimiento laparoscópico",
    cost: 3200,
    specialty: [{ text: "Cirugía General" }],
    providedBy: {
      reference: "Organization/1",
      display: "Hospital de Occidente"
    },
    location: [{ reference: "Location/16", display: "Quirófano Principal" }]
  },
  {
    id: "20",
    active: true,
    name: "Densitometría Ósea",
    abbreviation: "DMO",
    comment: "Evaluación de masa ósea lumbar y femoral",
    cost: 110,
    specialty: [{ text: "Reumatología" }, { text: "Radiología" }],
    providedBy: {
      reference: "Organization/13",
      display: "Centro de Osteoporosis"
    },
    location: [{ reference: "Location/17", display: "Unidad Densitometría" }]
  },
  {
    id: "21",
    active: true,
    name: "Terapia Ocupacional",
    abbreviation: "TO",
    comment: "Sesión de rehabilitación funcional",
    cost: 55,
    specialty: [{ text: "Terapia Ocupacional" }],
    providedBy: {
      reference: "Organization/10",
      display: "Centro de Rehabilitación"
    },
    location: [{ reference: "Location/18", display: "Sala de Terapia" }]
  },
  {
    id: "22",
    active: true,
    name: "Angiografía Coronaria",
    abbreviation: "ANGIO-COR",
    comment: "Estudio hemodinámico diagnóstico",
    cost: 2800,
    specialty: [{ text: "Cardiología Intervencionista" }],
    providedBy: {
      reference: "Organization/4",
      display: "Instituto Cardiológico"
    },
    location: [{ reference: "Location/19", display: "Hemocinética" }]
  },
  {
    id: "23",
    active: true,
    name: "Consulta Nutricional",
    abbreviation: "CON-NUTRI",
    comment: "Plan alimentario personalizado",
    cost: 70,
    specialty: [{ text: "Nutriología" }],
    providedBy: {
      reference: "Organization/14",
      display: "Centro de Nutrición"
    },
    location: [{ reference: "Location/20", display: "Consulta Nutrición" }]
  },
  {
    id: "24",
    active: true,
    name: "Ultrasonido Abdominal",
    abbreviation: "US-ABD",
    comment: "Evaluación de hígado, vesícula y páncreas",
    cost: 90,
    specialty: [{ text: "Radiología" }],
    providedBy: {
      reference: "Organization/2",
      display: "Clínica de Occidente"
    },
    location: [{ reference: "Location/21", display: "Ecografía 1" }]
  },
  {
    id: "25",
    active: true,
    name: "Prueba de COVID-19 PCR",
    abbreviation: "PCR-COVID",
    comment: "Resultados en 24 horas",
    cost: 75,
    specialty: [{ text: "Laboratorio Clínico" }, { text: "Infectología" }],
    providedBy: {
      reference: "Organization/1",
      display: "Hospital de Occidente"
    },
    location: [{ reference: "Location/22", display: "Laboratorio Molecular" }]
  }
];